#include "pch.h"
#include <vector>
#include "exports_graphics.h"
#include "whandle.h"
#include "exports.h"
#include "enums.h"

static GLuint cur_vao = 0;
static GLuint default_vbo = 0;
static GLuint cur_vbo = 0;
static void ensure_vao(GLuint vao);
static void ensure_vbo(GLuint vbo);

// make a vao
static GLuint make_vao(VertexElementType* type, int len);
static void* load_image(void* mem, int length, int* x, int* y, int* channels);
static void free_image(void* data);

extern "C"
{
    EXPORT void CALLCONV MsdgSwapBuffers(whandle* handle)
    {
        SwapBuffers(handle->hdc);
    }

    EXPORT void CALLCONV MsdgViewport(whandle* handle, int x, int y, int width, int height)
    {
        glViewport(x, y, width, height);
    }

    EXPORT void CALLCONV MsdgClear(whandle* handle, Color c)
    {
        glClearColor(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, c.a / 255.0f);
        glClear(GL_COLOR_BUFFER_BIT);
    }

    EXPORT void* CALLCONV MsdgRegisterVertexType(whandle*, VertexElementType* type, int len)
    {
        vertex_type_handle* h = small_alloc<vertex_type_handle>();
        h->type_ptr = type;
        h->length = len;
        h->default_vao_id = 0;
        GL_CHECK_ERROR;
        return h;
    }

    EXPORT void CALLCONV MsdgDrawPrimitives(whandle*,
        vertex_type_handle* vertex_type, PrimitiveType pt,
        void* data, int data_size, int vertices_to_draw
    )
    {
        ensure_vbo(default_vbo);
        if (vertex_type->default_vao_id == 0)
            vertex_type->default_vao_id = make_vao(vertex_type->type_ptr, vertex_type->length);
        ensure_vao(vertex_type->default_vao_id);

        glBufferData(GL_ARRAY_BUFFER, data_size, data, GL_DYNAMIC_DRAW);
        glDrawArrays(PrimitiveType_get_glinfo(pt), 0, vertices_to_draw);
    }

    EXPORT buffer_handle* CALLCONV MsdgCreateVertexBuffer(whandle*, vertex_type_handle* vertex_type)
    {
        GLuint id;
        glGenBuffers(1, &id);
        ensure_vbo(id);
        buffer_handle* h = new buffer_handle;
        h->vbo_id = id;
        h->vao_id = make_vao(vertex_type->type_ptr, vertex_type->length);
        return h;
    }

    EXPORT void CALLCONV MsdgDeleteVertexBuffer(whandle*, buffer_handle* buffer)
    {
        glDeleteVertexArrays(1, &buffer->vao_id);
        glDeleteBuffers(1, &buffer->vbo_id);
        GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgSetVertexBufferData(whandle*,
        buffer_handle* buffer_handle,
        void* data, int dataSize,
        VertexBufferDataUsage data_usage)
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glBufferData(GL_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_get_glinfo(data_usage));
        GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgDrawBufferPrimitives(whandle* whandle,
        buffer_handle* buffer_handle, PrimitiveType primitiveType,
        int verticesCount)
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glDrawArrays(PrimitiveType_get_glinfo(primitiveType), 0, verticesCount);
        GL_CHECK_ERROR;
    }

    EXPORT void* CALLCONV MsdgCreateTexture(whandle* whandle, int width, int height)
    {
        GLuint tex;
        glGenTextures(1, &tex);
        glBindTexture(GL_TEXTURE_2D, tex);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        const float borderColor[] = { 0.0f };
        glTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, borderColor);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);
        return (void*)tex;
    }

    // TODO support not only RGBA but also other formats
    EXPORT void CALLCONV MsdgSetTextureData(whandle*, void* texHandle, int width, int height, void* data, int channels)
    {
        GLuint tex = (GLuint)texHandle;
        glBindTexture(GL_TEXTURE_2D, tex);
        int align = (width % 2 == 0) ? 8 : 4;
        glPixelStorei(GL_UNPACK_ALIGNMENT, align);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
        GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgSetTexture(whandle*, int index, void* texHandle)
    {
        glActiveTexture(GL_TEXTURE0 + index);
        glBindTexture(GL_TEXTURE_2D, (GLuint)texHandle);
    }

    EXPORT void* CALLCONV MsdLoadImage(void* mem, int length, int* x, int* y, int* channels)
    {
        return load_image(mem, length, x, y, channels);
    }

    EXPORT void CALLCONV MsdFreeImage(void* texData)
    {
        free_image(texData);
    }
}

// hm, just temporarily use stb_image cuz it's head-only
// you can always switch to other libraries you like easily here
static void* load_image(void* mem, int length, int* x, int* y, int* channels)
{
    return stbi_load_from_memory((stbi_uc*)mem, length, x, y, channels, 0);
}

static void free_image(void* data)
{
    stbi_image_free(data);
}

void window_gl_init()
{
    glGenBuffers(1, &default_vbo);

    // create our default pos-color-tex shader
    const char* vshSource = R"(
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTex;
out vec4 vColor;
out vec2 vTex;

void main()
{
    vColor = aColor;
    vTex = aTex;
    gl_Position = vec4(aPos, 1.0);
}
)";
    const char* fshSource = R"(
#version 330 core
out vec4 FragColor;
in vec4 vColor;
in vec2 vTex;

uniform sampler2D tex0;

void main()
{
    FragColor = texture(tex0, vTex) * vColor;
} 
)";

    // TODO 'Shader' class

    GLuint vsh = glCreateShader(GL_VERTEX_SHADER);
    GLuint fsh = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(vsh, 1, &vshSource, nullptr);
    glShaderSource(fsh, 1, &fshSource, nullptr);
    glCompileShader(vsh);
    glCompileShader(fsh);
    GLuint prog = glCreateProgram();
    glAttachShader(prog, vsh);
    glAttachShader(prog, fsh);
    glLinkProgram(prog);
    glDeleteShader(vsh);
    glDeleteShader(fsh);
    glUseProgram(prog);
    glUniform1i(glGetUniformLocation(prog, "tex0"), 0);

    GL_CHECK_ERROR;
}

static void ensure_vao(GLuint vao)
{
    if (cur_vao != vao)
    {
        glBindVertexArray(vao);
        cur_vao = vao;
    }
}

static void ensure_vbo(GLuint vbo)
{
    if (cur_vbo != vbo)
    {
        assert(vbo != 0);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        cur_vbo = vbo;
    }
}

static GLuint make_vao(VertexElementType* type, int len)
{
    GL_CHECK_ERROR;
    assert(type != nullptr && len >= 1);
    assert(cur_vbo != 0);
    GLuint vao;
    glGenVertexArrays(1, &vao);
    glBindVertexArray(vao);
    cur_vao = vao;
    int singleVertexSize = 0;
    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        singleVertexSize += t.componentSize * t.count;
    }
    byte* currentOffset = 0;

    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        glVertexAttribPointer(i, t.count, t.type, GL_FALSE, singleVertexSize, (void*)currentOffset);
        glEnableVertexAttribArray(i);
        GL_CHECK_ERROR;

        currentOffset += t.componentSize * t.count;
    }
    return vao;
}