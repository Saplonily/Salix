#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <windowsx.h>
#include <vector>
#include <mutex>
#include <assert.h>
#include "common.h"
#include "keyboard.h"
#include "windowing.h"

msgloop_struct::msgloop_struct()
{
    event_list = new event_list_t();
    event_list->reserve(16);
    event_list_2 = new event_list_t();
    event_list_2->reserve(16);
}

msgloop_struct::~msgloop_struct()
{
    delete event_list;
    delete event_list_2;
}

EXPORT void CALLCONV MsdPollEvents(msd_window* win)
{
    HWND hwnd = win->hwnd;
    MSG msg{};
    while (PeekMessageW(&msg, hwnd, 0, 0, PM_REMOVE))
    {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
}

EXPORT event_list_t* CALLCONV MsdBeginProcessEvents(msd_window* win, size_t* count, win_event** events)
{
    msgloop_struct* m = &win->msgloop;

    assert(m->began_polling == false);
    event_list_t* temp = m->event_list;
    m->event_list = m->event_list_2;
    m->event_list_2 = temp;

    *count = m->event_list_2->size();
    *events = m->event_list_2->data();
    m->began_polling = true;
    return m->event_list_2;
}

EXPORT void CALLCONV MsdEndProcessEvents(msd_window* win, event_list_t* handle)
{
    assert(win->msgloop.began_polling == true);
    handle->clear();
    win->msgloop.began_polling = false;
}

#define push_event(e) { win->msgloop.event_list->push_back(e); }

#define make_check_button_down_case(wm, btn) \
    case wm:                                 \
    {                                        \
        we.type = event_type::pointer;       \
        we.arg1 = GET_X_LPARAM(lParam);      \
        we.arg2 = GET_Y_LPARAM(lParam);      \
        we.arg3.int16_left = btn;            \
        we.arg3.int16_right = 0;             \
        push_event(we);                      \
        SetCapture(hwnd);                    \
        return 0;                            \
    }                                        \

#define make_check_button_up_case(wm, btn)   \
    case wm:                                 \
    {                                        \
        we.type = event_type::pointer;       \
        we.arg1 = GET_X_LPARAM(lParam);      \
        we.arg2 = GET_Y_LPARAM(lParam);      \
        we.arg3.int16_left = btn;            \
        we.arg3.int16_right = 1;             \
        push_event(we);                      \
        ReleaseCapture();                    \
        return 0;                            \
    }                                        \

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam)
{
    msd_window* win = (msd_window*)GetWindowLongPtrW(hwnd, 0);
    if (!win) return DefWindowProcW(hwnd, uMsg, wParam, lParam);
    win_event we{};
    we.gc_handle = win->gc_handle;
    switch (uMsg)
    {
    case WM_USER_MSDCLOSE:
        DestroyWindow(hwnd);
        return 0;
    case WM_ERASEBKGND:
        return 0;
    case WM_CLOSE:
    {
        we.type = event_type::close;
        push_event(we);
        return 0;
    }
    case WM_MOVE:
    {
        int x = (int)(short)LOWORD(lParam);
        int y = (int)(short)HIWORD(lParam);
        we.type = event_type::move;
        we.arg1 = x;
        we.arg2 = y;
        push_event(we);
        return 0;
    }
    case WM_SIZE:
    {
        int width = (int)(short)LOWORD(lParam);
        int height = (int)(short)HIWORD(lParam);
        we.type = event_type::resize;
        we.arg1 = width;
        we.arg2 = height;
        push_event(we);
        return 0;
    }
    case WM_SYSKEYUP:
    case WM_SYSKEYDOWN:
        DefWindowProcW(hwnd, uMsg, wParam, lParam);
    case WM_KEYUP:
    case WM_KEYDOWN:
    {
        WORD vkCode = LOWORD(wParam);
        WORD keyFlags = HIWORD(lParam);
        WORD scanCode = LOBYTE(keyFlags);
        BOOL isExtendedKey = (keyFlags & KF_EXTENDED) == KF_EXTENDED;

        BOOL wasKeyDown = (keyFlags & KF_REPEAT) == KF_REPEAT;
        BOOL isKeyReleased = (keyFlags & KF_UP) == KF_UP;
        if (wasKeyDown && !isKeyReleased)
            break;

        // we can only received that PrintScreen is released
        if (vkCode == VK_SNAPSHOT)
        {
            Key key = vkCode_to_Key(vkCode);
            we.type = event_type::key_down;
            we.arg1 = (int32_t)key;
            push_event(we);
            we.type = event_type::key_up;
            push_event(we);
            break;
        }

        vkCode = MapVirtualKeyW(scanCode, MAPVK_VSC_TO_VK_EX);
        if (vkCode == VK_LCONTROL && isExtendedKey)
            vkCode = VK_RCONTROL;

        // TODO
        // For now, pressing the key NumPad7 will actually result in the key Home when the key NumLock is turned off.
        // But it's not very urgent at now.
        // Also, if you press both the LShift and RShift, LCtrl and RCtrl etc,
        // then the behaviour will be strange, also, it's not urgent at now.

        Key key = vkCode_to_Key(vkCode);
        we.type = !isKeyReleased ? event_type::key_down : event_type::key_up;
        we.arg1 = (int32_t)key;
        push_event(we);

        return 0;
    }
    case WM_SETFOCUS:
    {
        we.type = event_type::got_focus;
        push_event(we);
        return 0;
    }
    case WM_KILLFOCUS:
    {
        we.type = event_type::lost_focus;
        push_event(we);
        return 0;
    }

    make_check_button_down_case(WM_LBUTTONDOWN, 1);
    make_check_button_down_case(WM_RBUTTONDOWN, 2);
    make_check_button_down_case(WM_MBUTTONDOWN, 3);
    make_check_button_up_case(WM_LBUTTONUP, 1);
    make_check_button_up_case(WM_RBUTTONUP, 2);
    make_check_button_up_case(WM_MBUTTONUP, 3);

    case WM_MOUSEMOVE:
    {
        we.type = event_type::pointer;
        we.arg1 = GET_X_LPARAM(lParam);
        we.arg2 = GET_Y_LPARAM(lParam);
        we.arg3.int16_left = 0;
        we.arg3.int16_right = 2;
        push_event(we);
        return 0;
    }
    case WM_MOUSEWHEEL:
    {
        we.type = event_type::pointer_wheel;
        we.arg1 = GET_X_LPARAM(lParam);
        we.arg2 = GET_Y_LPARAM(lParam);
        we.arg3 = (int)GET_WHEEL_DELTA_WPARAM(wParam);
        push_event(we);
        return 0;
    }
    }
    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
#undef push_e
}