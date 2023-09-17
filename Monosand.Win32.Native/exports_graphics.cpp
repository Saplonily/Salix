#include "pch.h"
#include <vector>
#include "whandle.h"
#include "exports.h"
#include "enums.h"

extern "C"
{
    EXPORT void CALLCONV MsdgSwapBuffers(whandle* handle)
    {
        SwapBuffers(handle->hdc);
    }

    EXPORT void CALLCONV MsdgViewport(whandle* handle, int x, int y, int width, int height)
    {
        ensure_context(handle);
        glViewport(x, y, width, height);
    }

    struct Color { BYTE r, g, b, a; };
    EXPORT void CALLCONV MsdgClear(whandle* handle, Color c)
    {
        ensure_context(handle);
        glClearColor(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, c.a / 255.0f);
        glClear(GL_COLOR_BUFFER_BIT);
    }

    static GLuint cur_vao = 0;
    void ensure_vao(GLuint vao)
    {
        if (cur_vao != vao)
        {
            glBindVertexArray(vao);
            cur_vao = vao;
        }
    }

    static GLuint default_vbo = 0;
    static GLuint cur_vbo = 0;
    void ensure_vbo(GLuint vbo)
    {
        if (cur_vbo != default_vbo)
        {
            assert(default_vbo != 0);

            glBindBuffer(GL_ARRAY_BUFFER, default_vbo);
            cur_vbo = default_vbo;
        }
    }

#pragma warning (disable:4312) // 'int' converted to 'void*'
    EXPORT uint32_t CALLCONV MsdgRegisterVertexType(whandle* handle, VertexElementType* type, int len)
    {
        ensure_context(handle);
        ensure_vbo(default_vbo);
        assert(type != nullptr && len >= 1);
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
        int currentOffset = 0;

        for (int i = 0; i < len; i++)
        {
            vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
            glVertexAttribPointer(i, t.count, t.type, GL_FALSE, singleVertexSize, (void*)currentOffset);
            GL_CHECK_ERROR
            glEnableVertexAttribArray(i);
            
            currentOffset += t.componentSize * t.count;
        }
        GL_CHECK_ERROR
        return vao;
    }

    EXPORT void CALLCONV MsdgDrawPrimitives(whandle* handle, uint32_t vertexType, PrimitiveType pt, void* data, int dataSize, int verticesToDraw)
    {
        ensure_context(handle);
        ensure_vao(vertexType);
        ensure_vbo(default_vbo);

        glBufferData(GL_ARRAY_BUFFER, dataSize, data, GL_DYNAMIC_DRAW);
        glDrawArrays(PrimitiveType_get_glinfo(pt), 0, verticesToDraw);
        cur_vao = 0;
        cur_vbo = 0;
        glBindVertexArray(0);
        glBindBuffer(GL_ARRAY_BUFFER, 0);
    }
}

void window_gl_init()
{
    glGenBuffers(1, &default_vbo);

    // create our dummy shader
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
} 
)";

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