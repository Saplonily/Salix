#include "pch.h"
#include <vector>
#include "exports_graphics.h"
#include "whandle.h"
#include "exports.h"
#include "enums.h"

static GLuint cur_vao = 0;
static GLuint cur_vbo = 0;
static GLuint default_vbo = 0;
static void ensure_vao(GLuint vao);
static void ensure_vbo(GLuint vbo);

// make a vao
static GLuint make_vao(VertexElementType* type, int len);
static void* load_image(void* mem, int length, int* x, int* y, int* data_length, ImageFormat* format);
static void free_image(void* data);

extern "C"
{
    EXPORT GraphicsBackend CALLCONV MsdgGetGraphicsBackend()
    {
        return GraphicsBackend::Opengl33;
    }

    EXPORT void CALLCONV MsdgSwapBuffers(whandle* handle)
    {
        SwapBuffers(handle->hdc);
    }

    EXPORT void CALLCONV MsdgViewport(whandle* handle, int x, int y, int width, int height)
    {
        glViewport(x, y, width, height);
        GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgClear(whandle* handle, Color c)
    {
        glClearColor(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, c.a / 255.0f);
        GL_CHECK_ERROR;
        glClear(GL_COLOR_BUFFER_BIT);
        GL_CHECK_ERROR;
    }

    EXPORT void* CALLCONV MsdgRegisterVertexType(whandle*, VertexElementType* type, int len)
    {
        vertex_type_handle* h = small_alloc<vertex_type_handle>();
        h->type_ptr = type;
        h->length = len;
        h->default_vao_id = 0;
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
        GL_CHECK_ERROR;
        glDrawArrays(PrimitiveType_get_glinfo(pt), 0, vertices_to_draw);
        GL_CHECK_ERROR;
    }

    EXPORT buffer_handle* CALLCONV MsdgCreateVertexBuffer(whandle*, vertex_type_handle* vertex_type, byte use_ibo)
    {
        GLuint id;
        glGenBuffers(1, &id);
        GL_CHECK_ERROR;
        ensure_vbo(id);
        buffer_handle* h = new buffer_handle;
        h->vbo_id = id;
        h->vao_id = make_vao(vertex_type->type_ptr, vertex_type->length);
        h->ibo_id = 0;
        if (use_ibo)
        {
            glGenBuffers(1, &h->ibo_id); GL_CHECK_ERROR;
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, h->ibo_id); GL_CHECK_ERROR;
        }
        return h;
    }

    EXPORT void CALLCONV MsdgDeleteVertexBuffer(whandle*, buffer_handle* buffer)
    {
        glDeleteVertexArrays(1, &buffer->vao_id);
        GL_CHECK_ERROR;
        glDeleteBuffers(1, &buffer->vbo_id);
        GL_CHECK_ERROR;
        if (buffer->ibo_id)
            glDeleteBuffers(1, &buffer->ibo_id);
    }

    EXPORT void CALLCONV MsdgSetVertexBufferData(whandle*, buffer_handle* buffer_handle,
        void* data, int dataSize, VertexBufferDataUsage data_usage
    )
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glBufferData(GL_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_to_gl(data_usage));
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

    EXPORT void CALLCONV MsdgSetIndexBufferData(whandle*, buffer_handle* buffer_handle,
        void* data, int dataSize, VertexBufferDataUsage data_usage
    )
    {
        assert(buffer_handle->ibo_id != 0);
        ensure_vao(buffer_handle->vao_id);
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_to_gl(data_usage));
        GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgDrawIndexedBufferPrimitives(whandle*,
        buffer_handle* buffer_handle, PrimitiveType primitiveType,
        int verticesCount
    )
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glDrawElements(PrimitiveType_get_glinfo(primitiveType), verticesCount, GL_UNSIGNED_SHORT, 0);
        GL_CHECK_ERROR;
    }

    EXPORT void* CALLCONV MsdgCreateTexture(whandle* whandle, int width, int height)
    {
        GLuint tex;
        glGenTextures(1, &tex); GL_CHECK_ERROR;
        glBindTexture(GL_TEXTURE_2D, tex); GL_CHECK_ERROR;
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER); GL_CHECK_ERROR;
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER); GL_CHECK_ERROR;
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR); GL_CHECK_ERROR;
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR); GL_CHECK_ERROR;
        const float borderColor[] = { 0.0f, 0.0f, 0.0f, 0.0f };
        glTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, borderColor); GL_CHECK_ERROR;
        return (void*)(size_t)tex;
    }

    // TODO support not only RGBA but also other formats
    EXPORT void CALLCONV MsdgSetTextureData(whandle*, void* tex_handle, int width, int height, void* data, ImageFormat imageFormat)
    {
        GLuint tex = (GLuint)(size_t)tex_handle;
        glBindTexture(GL_TEXTURE_2D, tex);
        int lineWidth = (width * ImageFormat_get_size(imageFormat));
        int align = lineWidth % 8 == 0 ? 8 : lineWidth % 4 == 0 ? 4 : lineWidth % 2 == 0 ? 2 : 1;
        glPixelStorei(GL_UNPACK_ALIGNMENT, align); GL_CHECK_ERROR;
        GLenum format = ImageFormat_to_gl(imageFormat);
        glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data); GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MSdgDeleteTexture(whandle*, void* tex_handle)
    {
        GLuint tex = (GLuint)(size_t)tex_handle;
        assert(tex != 0);
        glDeleteTextures(1, &tex); GL_CHECK_ERROR;
    }

    EXPORT void* CALLCONV MsdLoadImage(void* mem, int length, int* x, int* y, int* data_length, ImageFormat* format)
    {
        return load_image(mem, length, x, y, data_length, format);
    }

    EXPORT void CALLCONV MsdFreeImage(void* texData)
    {
        free_image(texData);
    }

    EXPORT void CALLCONV MsdgSetTexture(whandle*, int index, void* tex_handle)
    {
        glActiveTexture(GL_TEXTURE0 + index); GL_CHECK_ERROR;
        glBindTexture(GL_TEXTURE_2D, (GLuint)(size_t)tex_handle); GL_CHECK_ERROR;
    }

    EXPORT void* CALLCONV MsdgCreateShaderFromGlsl(whandle*, const char* vert_source, const char* frag_source)
    {
        GLuint vsh = glCreateShader(GL_VERTEX_SHADER); GL_CHECK_ERROR;
        GLuint fsh = glCreateShader(GL_FRAGMENT_SHADER); GL_CHECK_ERROR;
        glShaderSource(vsh, 1, &vert_source, nullptr); GL_CHECK_ERROR;
        glShaderSource(fsh, 1, &frag_source, nullptr); GL_CHECK_ERROR;
        glCompileShader(vsh); GL_CHECK_ERROR;
        glCompileShader(fsh); GL_CHECK_ERROR;
        GLuint prog = glCreateProgram(); GL_CHECK_ERROR;
        glAttachShader(prog, vsh); GL_CHECK_ERROR;
        glAttachShader(prog, fsh); GL_CHECK_ERROR;
        glLinkProgram(prog); GL_CHECK_ERROR;
        glDeleteShader(vsh); GL_CHECK_ERROR;
        glDeleteShader(fsh); GL_CHECK_ERROR;

        return (void*)(size_t)prog;
    }

    EXPORT void CALLCONV MsdgDeleteShader(whandle*, void* shader_handle)
    {
        GLuint prog = (GLuint)(size_t)shader_handle;
        assert(prog != 0);
        glDeleteShader(prog); GL_CHECK_ERROR;
    }

    static GLuint cur_shd = 0;
    EXPORT void CALLCONV MsdgSetShader(whandle*, void* shader_handle)
    {
        GLuint prog = (GLuint)(size_t)shader_handle;
        if (cur_shd != prog)
        {
            glUseProgram(prog); GL_CHECK_ERROR;
            cur_shd = prog;
        }
    }

#pragma region uniform 'set's

    EXPORT int CALLCONV MsdgGetShaderParamLocation(whandle*, void* shaderHandle, const char* nameUtf8)
    {
        return glGetUniformLocation((GLuint)(size_t)shaderHandle, nameUtf8); GL_CHECK_ERROR;
    }

    EXPORT void CALLCONV MsdgSetShaderParamInt(whandle*, int loc, int value) { glUniform1i(loc, value); GL_CHECK_ERROR; }
    EXPORT void CALLCONV MsdgSetShaderParamFloat(whandle*, int loc, float value) { glUniform1f(loc, value); GL_CHECK_ERROR; }
    EXPORT void CALLCONV MsdgSetShaderParamVec4(whandle*, int loc, float* vec) { glUniform4fv(loc, 1, vec); GL_CHECK_ERROR; }
    EXPORT void CALLCONV MsdgSetShaderParamMat4(whandle*, int loc, float* mat, byte transpose)
    {
        glUniformMatrix4fv(loc, 1, transpose, mat); GL_CHECK_ERROR;
    }

#pragma endregion
}

// hm, just temporarily use stb_image cuz it's head-only
// you can always switch to other libraries you like easily here
static void* load_image(void* mem, int length, int* x, int* y, int* data_length, ImageFormat* format)
{
    int channels;
    void* data = stbi_load_from_memory((stbi_uc*)mem, length, x, y, &channels, 0);
    switch (channels)
    {
    case 1: *format = ImageFormat::R8; break;
    case 3: *format = ImageFormat::Rgb24; break;
    case 4: *format = ImageFormat::Rgba32; break;
    default: *format = (ImageFormat)-1; break;
    }
    *data_length = *x * *y * channels;
    return data;
}

static void free_image(void* data)
{
    stbi_image_free(data);
}

void window_gl_init()
{
    glGenBuffers(1, &default_vbo);
    GL_CHECK_ERROR;
}

static void ensure_vao(GLuint vao)
{
    if (cur_vao != vao)
    {
        glBindVertexArray(vao); GL_CHECK_ERROR;
        cur_vao = vao;
    }
}

static void ensure_vbo(GLuint vbo)
{
    if (cur_vbo != vbo)
    {
        assert(vbo != 0);
        glBindBuffer(GL_ARRAY_BUFFER, vbo); GL_CHECK_ERROR;
        cur_vbo = vbo;
    }
}


static GLuint make_vao(VertexElementType* type, int len)
{
    assert(type != nullptr && len >= 1);
    assert(cur_vbo != 0);
    GLuint vao;
    glGenVertexArrays(1, &vao); GL_CHECK_ERROR;
    glBindVertexArray(vao); GL_CHECK_ERROR;
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
        glVertexAttribPointer(i, t.count, t.type, GL_FALSE, singleVertexSize, (void*)currentOffset); GL_CHECK_ERROR;
        glEnableVertexAttribArray(i); GL_CHECK_ERROR;
        currentOffset += t.componentSize * t.count;
    }
    return vao;
}