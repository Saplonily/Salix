#include "api_render_context.h"
#include "api_windowing.h"
#include "api_graphics.h"

#include <glad/glad.h>
#include <glad/glad_wgl.h>

s_bool slxapi_render_context_init()
{
    WNDCLASSW wc{};
    wc.lpfnWndProc = DefWindowProcW;
    wc.lpszClassName = L"rccfg";
    ATOM atom = RegisterClassW(&wc);
    SLX_FAIL_COND(!atom, error_code::register_window_failed);

    return false;
}

SLX_API opengl_render_context* SLX_CALLCONV SLX_CreateRenderContext()
{
    HWND dummyHwnd = nullptr;
    HDC hdc = nullptr;
    HGLRC hglrc = nullptr;
    opengl_render_context* rc = nullptr;

    // this function can only be called once for now
    SLX_FAIL_COND_NULL(glViewport != nullptr, error_code::context_created_twice);

    dummyHwnd = CreateWindowExW(0, L"rccfg", L"", 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL);
    SLX_FAIL_COND_GOTO(!dummyHwnd, error_code::platform_error, failed);

    hdc = GetDC(dummyHwnd);
    SLX_FAIL_COND_GOTO(!hdc, error_code::platform_error, failed);

    int pixelFormat = ChoosePixelFormat(hdc, &pixelFormatDescriptor);
    SLX_FAIL_COND_GOTO(!pixelFormat, error_code::platform_error, failed);

    if (!SetPixelFormat(hdc, pixelFormat, &pixelFormatDescriptor))
        SLX_FAIL_GOTO(error_code::platform_error, failed);

    hglrc = wglCreateContext(hdc);
    SLX_FAIL_COND_GOTO(!hglrc, error_code::platform_error, failed);

    if (!wglMakeCurrent(hdc, hglrc))
        goto failed;

    if (!gladLoadGL() || !gladLoadWGL(hdc))
        SLX_FAIL_GOTO(error_code::context_gl_load_failed, failed);

    wglMakeCurrent(nullptr, nullptr);
    wglDeleteContext(hglrc);

    GLint attribs[] =
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
    SLX_FAIL_COND_GOTO(!hglrc, error_code::platform_error, failed);

    rc = new opengl_render_context();
    rc->hglrc = hglrc;

    wglMakeCurrent(hdc, hglrc);
    current_context = rc;

    graphics_initialize();

    wglMakeCurrent(nullptr, nullptr);
    current_context = nullptr;
    ReleaseDC(dummyHwnd, hdc);
    DestroyWindow(dummyHwnd);

#ifdef SLX_DEBUG
    if (!GLAD_GL_ARB_debug_output)
        SLX_FAIL_GOTO(error_code::context_gl_debug_output_not_supported, failed);
    glDebugMessageCallbackARB(gl_debug_callback, hglrc);
#endif
    if (!GLAD_WGL_EXT_swap_control)
        SLX_FAIL_GOTO(error_code::context_gl_swap_control_not_supported, failed);

    return rc;

failed:
    if (hglrc) wglDeleteContext(hglrc);
    if (hdc) ReleaseDC(dummyHwnd, hdc);
    if (dummyHwnd) DestroyWindow(dummyHwnd);
    if (rc) delete rc;
    return nullptr;
}

SLX_API s_bool SLX_CALLCONV SLX_AttachRenderContext(P_IN msd_window* win, P_IN opengl_render_context* rc)
{
    if (!wglMakeCurrent(win->hdc, rc->hglrc))
        SLX_FAIL(error_code::platform_error);
    current_context = rc;
    return false;
}

SLX_API void SLX_CALLCONV SLX_SwapBuffers(P_IN msd_window* win)
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

#ifdef SLX_DEBUG

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
#ifdef SLX_COMPATIBILITY_GL
    if (type == GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR_ARB)
        return;
#endif // SLX_COMPATIBILITY_GL
    const char* sourceStr = message_source_to_string(source);
    if (!length)
        length = (GLsizei)strlen(msg);

    fprintf(stderr, "[%s] ", sourceStr);
    fwrite(msg, sizeof(GLchar), length, stderr);
    if (length && msg[length - 1] != '\n')
        putc('\n', stderr);
    __debugbreak();
}

#endif