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
        GL_CHECK_ERROR
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

    EXPORT void* CALLCONV MsdgCreateVertexBuffer(whandle*, vertex_type_handle* vertex_type)
    {
        GLuint id;
        glGenBuffers(1, &id);
        ensure_vbo(id);
        buffer_handle* h = new buffer_handle;
        h->vbo_id = id;
        h->vao_id = make_vao(vertex_type->type_ptr, vertex_type->length); 
        return h;
    }

    EXPORT void CALLCONV MsdgSetVertexBufferData(whandle*, buffer_handle* buffer_handle,
        void* data, int dataSize,
        VertexBufferDataUsage data_usage)
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glBufferData(GL_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_get_glinfo(data_usage));
        GL_CHECK_ERROR
    }

    EXPORT void CALLCONV MsdgDrawBufferPrimitives(whandle* whandle,
        buffer_handle* buffer_handle, PrimitiveType primitiveType,
        int verticesCount)
    {
        ensure_vbo(buffer_handle->vbo_id);
        ensure_vao(buffer_handle->vao_id);
        glDrawArrays(PrimitiveType_get_glinfo(primitiveType), 0, verticesCount);
        GL_CHECK_ERROR
    }
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

void main()
{
    vColor = aColor;
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}
)";
    const char* fshSource = R"(
#version 330 core
in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
    //FragColor = vec4(1.0,1.0,1.0,1.0);
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

    int  success;
    char infoLog[512];
    glGetShaderiv(vsh, GL_COMPILE_STATUS, &success);
    if (!success)
    {
        glGetShaderInfoLog(vsh, 512, NULL, infoLog);
        printf("ERROR::SHADER::VERTEX::COMPILATION_FAILED\n%s\n", infoLog);
    }
    glGetShaderiv(fsh, GL_COMPILE_STATUS, &success);
    if (!success)
    {
        glGetShaderInfoLog(fsh, 512, NULL, infoLog);
        printf("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n%s\n", infoLog);
    }

    GL_CHECK_ERROR
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
    GL_CHECK_ERROR
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
        GL_CHECK_ERROR

        currentOffset += t.componentSize * t.count;
    }
    return vao;
}