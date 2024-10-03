#pragma once
#ifndef H_API_WINDOWING
#define H_API_WINDOWING

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <stdint.h>

#include "list.h"
#include "common.h"
#include "error.h"

#define WM_USER_SLXCLOSE (WM_USER + 0x01)

enum class WindowEventType : int32_t
{
    Close = 1,
    Move,
    Resize,
    KeyDown,
    KeyUp,
    GotFocus,
    LostFocus,
    // arg1: x
    // arg2: y
    // arg3:
    //   left int16:  type, 0:none, 1:left, 2:right, 3:middle
    //   right int16: 0:down, 1:up, 2:moved
    Mouse,
    MouseWheel
};

struct windowEvent
{
    WindowEventType type;
    int32_t arg1;
    int32_t arg2;
    union arg3Union
    {
        arg3Union() { int32 = 0; };
        arg3Union(int32_t int32) { this->int32 = int32; }
        int32_t int32;
        struct
        {
            int16_t int16Left;
            int16_t int16Right;
        };
    } arg3;
};

using event_list_t = List<windowEvent>;

struct windowMsgLoop
{
    bool beganPolling = false;
    event_list_t* eventList;
    event_list_t* eventList2;
    windowMsgLoop();
    ~windowMsgLoop();
};

struct slxWindow
{
    HWND hwnd;
    HDC hdc;
    windowMsgLoop msgloop;

    slxWindow(HWND hwnd, HDC hdc) :
        hwnd(hwnd), hdc(hdc)
    {
    }
};

extern PIXELFORMATDESCRIPTOR pixelFormatDescriptor;

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);

s_bool slxWindowingInit();

SLX_API slxWindow* SLX_CALLCONV SLX_CreateWindow(int32_t width, int32_t height, P_IN wchar_t* title);
SLX_API void SLX_CALLCONV SLX_DestroyWindow(P_IN slxWindow* win);
SLX_API void SLX_CALLCONV SLX_ShowWindow(P_IN slxWindow* win);
SLX_API void SLX_CALLCONV SLX_HideWindow(P_IN slxWindow* win);
SLX_API void SLX_CALLCONV SLX_GetWindowRect(P_IN slxWindow* win, P_OUT RECT* outRect);
SLX_API void SLX_CALLCONV SLX_SetWindowSize(P_IN slxWindow* win, int width, int height);
SLX_API void SLX_CALLCONV SLX_SetWindowPos(P_IN slxWindow* win, int x, int y);
SLX_API void SLX_CALLCONV SLX_SetWindowTitle(P_IN slxWindow* win, P_IN wchar_t* outTitle);
SLX_API int SLX_CALLCONV SLX_GetWindowTitle(P_IN slxWindow* win, P_OUT wchar_t* outTitle);

SLX_API void SLX_CALLCONV SLX_PollEvents(P_IN slxWindow* win);
SLX_API event_list_t* SLX_CALLCONV SLX_BeginProcessEvents(P_IN slxWindow* win, P_OUT size_t* count, P_OUT windowEvent** events);
SLX_API void SLX_CALLCONV SLX_EndProcessEvents(P_IN slxWindow* win, P_IN event_list_t* handle);

#endif