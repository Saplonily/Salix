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

static PIXELFORMATDESCRIPTOR pixelFormatDescriptor;

void windowing_initialize()
{
    WNDCLASSW wc{};
    wc.lpfnWndProc = WindowProc;
    wc.hCursor = LoadCursorW(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW);
    wc.lpszClassName = Monosand;
    // | msd_window* |
    wc.cbWndExtra = sizeof(void*);
    RegisterClassW(&wc);

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
}

// TODO: error handle
EXPORT HGLRC MsdCreateRenderContext()
{
    // this function can only be called once
    assert(glViewport == 0);

    wchar_t chr = L'\0';
    HWND dummyHwnd = CreateWindowExW(0, &chr, &chr, 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL);
    HDC hdc = GetDC(dummyHwnd);
    int pixelFormat = ChoosePixelFormat(hdc, &pixelFormatDescriptor);
    SetPixelFormat(hdc, pixelFormat, &pixelFormatDescriptor);

    HGLRC hglrc = wglCreateContext(hdc);
    wglMakeCurrent(hdc, hglrc);
    gladLoadGL();
    gladLoadWGL(hdc);
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
    // TODO error handling
    assert(hglrc != nullptr);
    assert(GLAD_WGL_EXT_swap_control);
#if _DEBUG
    glDebugMessageCallbackARB(gl_debug_callback, hglrc);
#endif
    render_context_graphics_init();
    ReleaseDC(dummyHwnd, hdc);
    DestroyWindow(dummyHwnd);
    wglMakeCurrent(nullptr, nullptr);
    return hglrc;
}

EXPORT void MsdAttachRenderContext(msd_window* win, HGLRC hglrc)
{
    wglMakeCurrent(win->hdc, hglrc);
}

// TODO: initial position
// TODO: window style
EXPORT msd_window* CALLCONV MsdCreateWindow(int32_t width, int32_t height, wchar_t* title, void* gc_handle)
{
    RECT rect{ 0, 0, width, height };
    AdjustWindowRect(&rect, WS_OVERLAPPEDWINDOW, FALSE);
    HWND hwnd = CreateWindowExW(0L, Monosand, title, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        rect.right - rect.left, rect.bottom - rect.top,
        NULL, NULL, NULL, NULL
    );

    ShowWindow(hwnd, SW_HIDE);
    UpdateWindow(hwnd);

    // TODO impl error handler
    HDC hdc = GetDC(hwnd);
    int pixelFormat = ChoosePixelFormat(hdc, &pixelFormatDescriptor);
    SetPixelFormat(hdc, pixelFormat, &pixelFormatDescriptor);
    msd_window* win = new msd_window();
    win->gc_handle = gc_handle;
    win->hwnd = hwnd;
    win->hdc = hdc;
    SetWindowLongPtrW(hwnd, 0, (LONG_PTR)win);

    return win;
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