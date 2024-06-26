#include "api_windowing.h"

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <timeapi.h>

#include <glad/glad.h>
#include <glad/glad_wgl.h>

PIXELFORMATDESCRIPTOR pixelFormatDescriptor;

s_bool slxapi_windowing_init()
{
    WNDCLASSW wc{};
    wc.lpfnWndProc = WindowProc;
    wc.hCursor = LoadCursorW(nullptr, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW);
    wc.lpszClassName = FrameworkName;
    wc.cbWndExtra = sizeof(msd_window*);
    ATOM atom = RegisterClassW(&wc);
    SLX_FAIL_COND(!atom, error_code::register_window_failed);

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

    // TODO: imm supports
    ImmDisableIME(NULL);
    return 0;
}

// TODO: initial position
// TODO: window style
// TODO: window_config struct
SLX_API msd_window* SLX_CALLCONV SLX_CreateWindow(int32_t width, int32_t height, P_IN wchar_t* title, void* gc_handle)
{
    msd_window* win = nullptr;
    RECT rect{ 0, 0, width, height };
    AdjustWindowRect(&rect, WS_OVERLAPPEDWINDOW, FALSE);
    HWND hwnd = CreateWindowExW(0L, FrameworkName, title, WS_OVERLAPPEDWINDOW,
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
    win = new msd_window(hwnd, hdc, gc_handle);
    SetWindowLongPtrW(hwnd, 0, (LONG_PTR)win);

    return win;
failed:
    if (hwnd) DestroyWindow(hwnd);
    if (win) delete win;
    SLX_FAIL_NULL(error_code::platform_error);
}

SLX_API void SLX_CALLCONV SLX_ShowWindow(P_IN msd_window* win)
{
    ShowWindow(win->hwnd, SW_NORMAL);
}

SLX_API void SLX_CALLCONV SLX_HideWindow(P_IN msd_window* win)
{
    ShowWindow(win->hwnd, SW_HIDE);
}

SLX_API void SLX_CALLCONV SLX_DestroyWindow(P_IN msd_window* win)
{
    PostMessageW(win->hwnd, WM_USER_SLXCLOSE, 0, 0);
}

SLX_API void SLX_CALLCONV SLX_GetWindowRect(P_IN msd_window* win, P_OUT RECT* out_rect)
{
    GetClientRect(win->hwnd, out_rect);
}

SLX_API void SLX_CALLCONV SLX_SetWindowSize(P_IN msd_window* win, int width, int height)
{
    RECT rect{ 0, 0, width, height };
    AdjustWindowRect(&rect, GetWindowLongPtrW(win->hwnd, GWL_STYLE), FALSE);
    SetWindowPos(win->hwnd, NULL, 0, 0, rect.right - rect.left, rect.bottom - rect.top, SWP_NOMOVE);
}

SLX_API void SLX_CALLCONV SLX_SetWindowPos(P_IN msd_window* win, int x, int y)
{
    RECT rect{ x, y, 0, 0 };
    AdjustWindowRect(&rect, GetWindowLongPtrW(win->hwnd, GWL_STYLE), FALSE);
    SetWindowPos(win->hwnd, NULL, rect.left, rect.top, 0, 0, SWP_NOSIZE);
}

SLX_API void SLX_CALLCONV SLX_SetWindowTitle(P_IN msd_window* win, P_OUT wchar_t* out_title)
{
    SetWindowTextW(win->hwnd, out_title);
}

SLX_API int SLX_CALLCONV SLX_GetWindowTitle(P_IN msd_window* win, P_OUT wchar_t* out_title)
{
    int len = GetWindowTextLengthW(win->hwnd);
    if (out_title != nullptr)
        GetWindowTextW(win->hwnd, out_title, len + 1);
    return len;
}