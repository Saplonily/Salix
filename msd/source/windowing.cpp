#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <timeapi.h>

#include <cstdint>
#include <assert.h>
#include <stdio.h>

#include <glad/glad.h>
#include <glad/glad_wgl.h>

#include "common.h"
#include "initializations.h"
#include "windowing.h"
#include "error_code.h"

static PIXELFORMATDESCRIPTOR pixelFormatDescriptor;

error_code windowing_initialize()
{
    WNDCLASSW wc{};
    wc.lpfnWndProc = WindowProc;
    wc.hCursor = LoadCursorW(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW);
    wc.lpszClassName = Monosand;
    // | msd_window* |
    wc.cbWndExtra = sizeof(void*);
    ATOM atom = RegisterClassW(&wc);
    if (!atom) return error_code::register_window_failed;

    pixelFormatDescriptor = {
        sizeof(PIXELFORMATDESCRIPTOR),
        1,                     // version number  
        PFD_DRAW_TO_WINDOW |   // support window  
        PFD_SUPPORT_OPENGL |   // support OpenGL  
        PFD_DOUBLEBUFFER,      // double buffered  
        PFD_TYPE_RGBA,         // RGBA type  
        24,                    // 24-bit color depth  
        0, 0, 0, 0, 0, 0,      // color bits ignored  
        0,                     // no alpha buffer  
        0,                     // shift bit ignored  
        0,                     // no accumulation buffer  
        0, 0, 0, 0,            // accum bits ignored  
        24,                    // 24-bit z-buffer      
        0,                     // no stencil buffer  
        0,                     // no auxiliary buffer  
        PFD_MAIN_PLANE,        // main layer  
        0,                     // reserved  
        0, 0, 0                // layer masks ignored  
    };

    timeBeginPeriod(1);
    ImmDisableIME(NULL); // TODO: imm support
    return error_code::none;
}
template struct m_result<HGLRC>;
EXPORT m_result<HGLRC> MsdCreateRenderContext()
{
    // this function can only be called once
    if (glViewport)
        return make_result<HGLRC>(error_code::create_render_context_twice);

    WCHAR c = '\0';
    HWND dummyHwnd = CreateWindowExW(0, Monosand, &c, 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL);
    if (!dummyHwnd) goto failed;

    HDC hdc = GetDC(dummyHwnd);
    if (!hdc) goto failed;

    int pixelFormat = ChoosePixelFormat(hdc, &pixelFormatDescriptor);
    if (!pixelFormat) goto failed;

    if (!SetPixelFormat(hdc, pixelFormat, &pixelFormatDescriptor)) goto failed;

    HGLRC hglrc = wglCreateContext(hdc);
    if (!hglrc) goto failed;

    if (!wglMakeCurrent(hdc, hglrc)) goto failed;
    if (!gladLoadGL()) goto failed;
    if (!gladLoadWGL(hdc)) goto failed;
    wglMakeCurrent(nullptr, nullptr);
    wglDeleteContext(hglrc);

    GLint attribs[] =
    {
        WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
        WGL_CONTEXT_MINOR_VERSION_ARB, 3,

    #ifndef MSDG_COMPATIBILITY_GL
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
    #else
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
    #endif
    #if _DEBUG
        WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_DEBUG_BIT_ARB,
    #endif
        0
    };
    hglrc = wglCreateContextAttribsARB(hdc, nullptr, attribs);
    wglMakeCurrent(hdc, hglrc);
#if _DEBUG
    if (!GLAD_GL_ARB_debug_output)
    {
        wglDeleteContext(hglrc);
        return make_result<HGLRC>(error_code::context_not_support_debug_output);
    }
    glDebugMessageCallbackARB(gl_debug_callback, hglrc);
#endif
    render_context_graphics_init();
    ReleaseDC(dummyHwnd, hdc);
    DestroyWindow(dummyHwnd);
    wglMakeCurrent(nullptr, nullptr);
    if (!GLAD_WGL_EXT_swap_control)
    {
        wglDeleteContext(hglrc);
        return make_result<HGLRC>(error_code::context_not_support_swap_control);
    }
    return make_result<HGLRC>(hglrc);
failed:
    return make_result<HGLRC>(error_code::create_render_context_failed, HRESULT_FROM_WIN32(GetLastError()));
}

EXPORT m_result<void> MsdAttachRenderContext(msd_window* win, HGLRC hglrc)
{
    if (!wglMakeCurrent(win->hdc, hglrc))
        return make_result(error_code::context_attach_failed, HRESULT_FROM_WIN32(GetLastError()));
    return make_result();
}

// TODO: initial position
// TODO: window style
template struct m_result<msd_window*>;
EXPORT m_result<msd_window*> CALLCONV MsdCreateWindow(int32_t width, int32_t height, wchar_t* title, void* gc_handle)
{
    RECT rect{ 0, 0, width, height };
    AdjustWindowRect(&rect, WS_OVERLAPPEDWINDOW, FALSE);
    HWND hwnd = CreateWindowExW(0L, Monosand, title, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        rect.right - rect.left, rect.bottom - rect.top,
        NULL, NULL, NULL, NULL
    );
    if (!hwnd) goto failed;

    ShowWindow(hwnd, SW_HIDE);
    UpdateWindow(hwnd);

    HDC hdc = GetDC(hwnd);
    int pixelFormat = ChoosePixelFormat(hdc, &pixelFormatDescriptor);
    if (!pixelFormat) goto failed;
    if (!SetPixelFormat(hdc, pixelFormat, &pixelFormatDescriptor)) goto failed;
    msd_window* win = new msd_window(hwnd, hdc, gc_handle);
    SetWindowLongPtrW(hwnd, 0, (LONG_PTR)win);

    return make_result<msd_window*>(win);
failed:
    return make_result<msd_window*>(error_code::create_window_failed, HRESULT_FROM_WIN32(GetLastError()));
}

EXPORT void CALLCONV MsdShowWindow(msd_window* win) { ShowWindow(win->hwnd, SW_NORMAL); }

EXPORT void CALLCONV MsdHideWindow(msd_window* win) { ShowWindow(win->hwnd, SW_HIDE); }

EXPORT void CALLCONV MsdDestroyWindow(msd_window* win)
{
    PostMessageW(win->hwnd, WM_USER_MSDCLOSE, 0, 0);
}

EXPORT RECT CALLCONV MsdGetWindowRect(msd_window* win)
{
    RECT rect{};
    GetClientRect(win->hwnd, &rect);
    return rect;
}

EXPORT void CALLCONV MsdSetWindowSize(msd_window* win, int width, int height)
{
    RECT rect{ 0, 0, width, height };
    AdjustWindowRect(&rect, GetWindowLongPtrW(win->hwnd, GWL_STYLE), FALSE);
    SetWindowPos(win->hwnd, NULL, 0, 0, rect.right - rect.left, rect.bottom - rect.top, SWP_NOMOVE);
}

EXPORT void CALLCONV MsdSetWindowPos(msd_window* win, int x, int y)
{
    RECT rect{ x, y, 0, 0 };
    AdjustWindowRect(&rect, GetWindowLongPtrW(win->hwnd, GWL_STYLE), FALSE);
    SetWindowPos(win->hwnd, NULL, rect.left, rect.top, 0, 0, SWP_NOSIZE);
}

EXPORT void CALLCONV MsdSetWindowTitle(msd_window* win, wchar_t* title)
{
    SetWindowTextW(win->hwnd, title);
}

EXPORT int CALLCONV MsdGetWindowTitle(msd_window* win, wchar_t* title)
{
    int len = GetWindowTextLengthW(win->hwnd);
    if (title != nullptr)
        GetWindowTextW(win->hwnd, title, len + 1);
    return len;
}