#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <windowsx.h>
#include <vector>
#include <assert.h>
#include "common.h"
#include "keyboard.h"

#define make_check_button_down_case(wm, btn) \
    case wm:                                 \
    {                                        \
        we.type = event::pointer;            \
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
        we.type = event::pointer;            \
        we.arg1 = GET_X_LPARAM(lParam);      \
        we.arg2 = GET_Y_LPARAM(lParam);      \
        we.arg3.int16_left = btn;            \
        we.arg3.int16_right = 1;             \
        push_event(we);                      \
        ReleaseCapture();                    \
        return 0;                            \
    }                                        \

#define push_event(e) { event_list->push_back(e); }

enum class event : int32_t
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
    event type;
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

static bool began_polling = false;
static event_list_t* event_list;
static event_list_t* event_list_2;

void windowing_msgloop_initialize()
{
    event_list = new event_list_t;
    event_list->reserve(16);
    event_list_2 = new event_list_t;
    event_list_2->reserve(16);
}

EXPORT event_list_t* CALLCONV MsdBeginPullEvents(HWND hwnd, size_t* count, win_event** events)
{
    assert(began_polling == false);
    // TODO message merging
    MSG msg{};
    while (PeekMessageW(&msg, hwnd, 0, 0, PM_REMOVE))
    {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }

    event_list_t* temp = event_list;
    event_list = event_list_2;
    event_list_2 = temp;

    *count = event_list_2->size();
    *events = event_list_2->data();
    began_polling = true;
    return event_list_2;
}

EXPORT void CALLCONV MsdEndPullEvents(HWND hwnd, event_list_t* handle)
{
    assert(began_polling == true);
    handle->clear();
    began_polling = false;
}

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam)
{
    void* gc_handle = (void*)GetWindowLongPtrW(hwnd, 0);
    win_event we{};
    we.gc_handle = gc_handle;
    switch (uMsg)
    {
    case WM_ERASEBKGND:
        return 1;

    case WM_CLOSE:
    {
        we.type = event::close;
        push_event(we);
        return 0;
    }
    case WM_MOVE:
    {
        int x = (int)(short)LOWORD(lParam);
        int y = (int)(short)HIWORD(lParam);
        we.type = event::move;
        we.arg1 = x;
        we.arg2 = y;
        push_event(we);
        return 0;
    }
    case WM_SIZE:
    {
        int width = (int)(short)LOWORD(lParam);
        int height = (int)(short)HIWORD(lParam);
        we.type = event::resize;
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
            we.type = event::key_down;
            we.arg1 = (int32_t)key;
            push_event(we);
            we.type = event::key_up;
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
        we.type = !isKeyReleased ? event::key_down : event::key_up;
        we.arg1 = (int32_t)key;
        push_event(we);

        return 0;
    }
    case WM_SETFOCUS:
    {
        we.type = event::got_focus;
        push_event(we);
        return 0;
    }
    case WM_KILLFOCUS:
    {
        we.type = event::lost_focus;
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
        we.type = event::pointer;
        we.arg1 = GET_X_LPARAM(lParam);
        we.arg2 = GET_Y_LPARAM(lParam);
        we.arg3.int16_left = 0;
        we.arg3.int16_right = 2;
        push_event(we);
        return 0;
    }
    case WM_MOUSEWHEEL:
    {
        we.type = event::pointer_wheel;
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