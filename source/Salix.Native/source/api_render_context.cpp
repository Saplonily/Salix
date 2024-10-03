#include "api_render_context.h"
#include "api_windowing.h"
#include "api_graphics.h"

#include <glad/glad.h>
#include <glad/glad_wgl.h>
#include <assert.h>

SLX_API HGLRC SLX_CALLCONV SLX_CreateRenderContext(P_IN slxWindow* win)
{
    HGLRC hglrc = nullptr;

    // this function can only be called once for now
    SLX_FAIL_COND_NULL(glad_glViewport != nullptr, ErrorCode::ContextCreatedTwice);

    HDC hdc = win->hdc;
    hglrc = wglCreateContext(hdc);
    SLX_FAIL_COND_GOTO(!hglrc, ErrorCode::PlatformError, failed);

    if (!wglMakeCurrent(hdc, hglrc))
        SLX_FAIL_GOTO(ErrorCode::PlatformError, failed);

    if (!gladLoadWGL(hdc))
        SLX_FAIL_GOTO(ErrorCode::ContextGLLoadFailed, failed);

    if (!GLAD_WGL_ARB_create_context || !GLAD_WGL_ARB_create_context_profile)
        SLX_FAIL_GOTO(ErrorCode::ContextWGLCreateContextNotSupported, failed);

    if (!GLAD_WGL_ARB_pixel_format)
        SLX_FAIL_GOTO(ErrorCode::ContextWGLPixelFormatNotSupported, failed);

    if (!GLAD_WGL_EXT_swap_control)
        SLX_FAIL_GOTO(ErrorCode::ContextSwapControlNotSupported, failed);

    wglMakeCurrent(nullptr, nullptr);
    wglDeleteContext(hglrc);

    int attribs[] =
    {
        WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
        WGL_CONTEXT_MINOR_VERSION_ARB, 3,
    #ifndef SLX_COMPATIBILITY_GL
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
    #else
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
    #endif
    #ifdef SLX_DEBUG
        WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_DEBUG_BIT_ARB,
    #endif
        0
    };

    hglrc = wglCreateContextAttribsARB(hdc, nullptr, attribs);
    SLX_FAIL_COND_GOTO(!hglrc, ErrorCode::PlatformError, failed);

    if (!wglMakeCurrent(hdc, hglrc))
        SLX_FAIL_GOTO(ErrorCode::PlatformError, failed);

    if (!gladLoadGL())
        SLX_FAIL_GOTO(ErrorCode::ContextGLLoadFailed, failed);

#ifdef SLX_DEBUG
    if (!GLAD_GL_ARB_debug_output)
        SLX_FAIL_GOTO(ErrorCode::ContextGLDebugOutputNotSupported, failed);
    glDebugMessageCallbackARB(glDebugCallbackHandler, hglrc);
#endif

    graphics_initialize();

    return hglrc;

failed:
    wglMakeCurrent(nullptr, nullptr);
    if (hglrc) wglDeleteContext(hglrc);
    return nullptr;
}

SLX_API void SLX_CALLCONV SLX_SwapBuffers(P_IN slxWindow* win)
{
    // FIXME some screen recorders may break our program here
    // for example, OCam and Bandicam
    // but others like XBox Capture and OBS won't

    // This problem can be resolved by setting
    // WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB to the WGL_CONTEXT_PROFILE_MASK_ARB,
    // for now it can be enabled by defining the SLX_COMPATIBILITY_GL macro.
    SwapBuffers(win->hdc);
}

SLX_API double SLX_CALLCONV SLX_GetVSyncFrameTime()
{
    HDC hdc = GetDC(NULL);
    int rate = GetDeviceCaps(hdc, VREFRESH);
    ReleaseDC(NULL, hdc);
    return 1.0 / rate;
}

SLX_API void SLX_CALLCONV SLX_SetVSyncEnabled(s_bool enable)
{
    wglSwapIntervalEXT(enable ? 1 : 0);
}

SLX_API s_bool SLX_CALLCONV SLX_QueryRenderContextInfo(P_OUT render_context_info* out_render_context_info)
{
    assert(out_render_context_info != nullptr);

    glGetIntegerv(GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS, &out_render_context_info->max_texture_units);

    return false;
}

#ifdef SLX_DEBUG
#include <cstdio>
static const char* messageSourceToString(GLenum source)
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

void APIENTRY glDebugCallbackHandler(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* userParam)
{
#ifdef SLX_COMPATIBILITY_GL
    if (type == GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR_ARB)
        return;
#endif // SLX_COMPATIBILITY_GL
    const char* sourceStr = messageSourceToString(source);
    if (!length)
        length = (GLsizei)strlen(msg);

    fprintf(stderr, "[%s] ", sourceStr);
    fwrite(msg, sizeof(GLchar), length, stderr);
    if (length && msg[length - 1] != '\n')
        putc('\n', stderr);
    __debugbreak();
}

#endif