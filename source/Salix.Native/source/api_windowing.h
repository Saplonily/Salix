#pragma once
#ifndef H_API_WINDOWING
#define H_API_WINDOWING

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <stdint.h>
#include <vector>

#include "common.h"
#include "error.h"

#define WM_USER_SLXCLOSE (WM_USER + 0x01)

enum class event_type : int32_t
{
    close = 1,
    move,
    resize,
    key_down,
    key_up,
    got_focus,
    lost_focus,
    // arg1: x
    // arg2: y
    // arg3:
    //   left int16:  type, 0:none, 1:left, 2:right, 3:middle
    //   right int16: 0:down, 1:up, 2:moved
    mouse,
    mouse_wheel
};

struct win_event
{
    event_type type;
    int32_t arg1;
    int32_t arg2;
    union arg3_union
    {
        arg3_union() { int32 = 0; };
        arg3_union(int32_t int32) { this->int32 = int32; }
        int32_t int32;
        struct
        {
            int16_t int16_left;
            int16_t int16_right;
        };
    } arg3;
    void* gc_handle;
};

using event_list_t = std::vector<win_event>;

struct win_msgloop
{
    bool began_polling = false;
    event_list_t* event_list;
    event_list_t* event_list_2;
    win_msgloop();
    ~win_msgloop();
};

struct msd_window
{
    HWND hwnd;
    HDC hdc;
    void* gc_handle;
    win_msgloop msgloop;

    msd_window(HWND hwnd, HDC hdc, void* gc_handle) :
        hwnd(hwnd), hdc(hdc), gc_handle(gc_handle)
    {
    }
};

extern PIXELFORMATDESCRIPTOR pixelFormatDescriptor;

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);

s_bool slxapi_windowing_init();

SLX_API msd_window* SLX_CALLCONV SLX_CreateWindow(int32_t width, int32_t height, P_IN wchar_t* title, void* gc_handle);
SLX_API void SLX_CALLCONV SLX_DestroyWindow(P_IN msd_window* win);
SLX_API void SLX_CALLCONV SLX_ShowWindow(P_IN msd_window* win);
SLX_API void SLX_CALLCONV SLX_HideWindow(P_IN msd_window* win);
SLX_API void SLX_CALLCONV SLX_GetWindowRect(P_IN msd_window* win, P_OUT RECT* out_rect);
SLX_API void SLX_CALLCONV SLX_SetWindowSize(P_IN msd_window* win, int width, int height);
SLX_API void SLX_CALLCONV SLX_SetWindowPos(P_IN msd_window* win, int x, int y);
SLX_API void SLX_CALLCONV SLX_SetWindowTitle(P_IN msd_window* win, P_OUT wchar_t* out_title);
SLX_API int SLX_CALLCONV SLX_GetWindowTitle(P_IN msd_window* win, P_OUT wchar_t* out_title);

SLX_API void SLX_CALLCONV SLX_PollEvents(P_IN msd_window* win);
SLX_API event_list_t* SLX_CALLCONV SLX_BeginProcessEvents(P_IN msd_window* win, P_OUT size_t* count, P_OUT win_event** events);
SLX_API void SLX_CALLCONV SLX_EndProcessEvents(P_IN msd_window* win, P_IN event_list_t* handle);

#endif