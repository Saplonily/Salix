#include "pch.h"
#if _DEBUG

static const char* message_source_to_string(GLenum source)
{
    switch (source)
    {
    case GL_DEBUG_SOURCE_API_ARB:
        return "OpenGL";
    case GL_DEBUG_SOURCE_WINDOW_SYSTEM_ARB:
        return "WindowSystem";
    case GL_DEBUG_SOURCE_SHADER_COMPILER_ARB:
        return "ShaderCompiler";
    case GL_DEBUG_SOURCE_THIRD_PARTY_ARB:
        return "ThridParty";
    case GL_DEBUG_SOURCE_APPLICATION_ARB:
        return "User";
    case GL_DEBUG_SOURCE_OTHER_ARB:
        return "OtherSource";
    default:
        return "UnknownSource";
    }
}

void APIENTRY gl_debug_callback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* userParam)
{
    const char* sourceStr = message_source_to_string(source);
    if (!length)
        length = (GLsizei)strlen(msg);
    
    putc('[', stderr);
    fputs(sourceStr, stderr);
    putc(']', stderr);
    putc(' ', stderr);
    fwrite(msg, sizeof(GLchar), length, stderr);
    if (length && msg[length - 1] != '\n')
        putc('\n', stderr);
}

#endif