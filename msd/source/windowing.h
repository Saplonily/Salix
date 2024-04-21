#pragma once
#ifndef H_WINDOWING
#define H_WINDOWING

#include <stdint.h>
#include <vector>
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <glad/glad.h>

#define WM_USER_MSDCLOSE (WM_USER + 0x01)

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);
#if _DEBUG
void APIENTRY gl_debug_callback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* userParam);
#endif

enum class event_type : int32_t
{
    close = 1,
    move = 3,
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
    pointer,
    pointer_wheel
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

struct msgloop_struct
{
    bool began_polling = false;
    event_list_t* event_list;
    event_list_t* event_list_2;
    msgloop_struct();
    ~msgloop_struct();
};

struct msd_window
{
    HWND hwnd;
    HDC hdc;
    void* gc_handle;
    msgloop_struct msgloop;
};

#endif