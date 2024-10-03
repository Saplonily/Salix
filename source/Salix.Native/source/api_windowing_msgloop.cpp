#include "api_windowing.h"

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <windowsx.h>

#include <vector>
#include <assert.h>

#include "common.h"
#include "keyboard.h"

windowMsgLoop::windowMsgLoop()
{
    eventList = new event_list_t(16);
    eventList2 = new event_list_t(16);
}

windowMsgLoop::~windowMsgLoop()
{
    delete eventList;
    delete eventList2;
}

SLX_API void SLX_CALLCONV SLX_PollEvents(P_IN slxWindow* win)
{
    HWND hwnd = win->hwnd;
    MSG msg{ };
    while (PeekMessageW(&msg, hwnd, 0, 0, PM_REMOVE))
    {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
}

SLX_API event_list_t* SLX_CALLCONV SLX_BeginProcessEvents(P_IN slxWindow* win, P_OUT size_t* count, P_OUT windowEvent** events)
{
    windowMsgLoop* m = &win->msgloop;

    assert(m->beganPolling == false);
    event_list_t* temp = m->eventList;
    m->eventList = m->eventList2;
    m->eventList2 = temp;

    *count = m->eventList2->size();
    *events = m->eventList2->data();
    m->beganPolling = true;
    return m->eventList2;
}

SLX_API void SLX_CALLCONV SLX_EndProcessEvents(P_IN slxWindow* win, P_IN event_list_t* handle)
{
    assert(win->msgloop.beganPolling == true);
    handle->clear();
    win->msgloop.beganPolling = false;
}

#define push_event(e) { win->msgloop.eventList->add(e); }

#define check_button_down_case(wm, btn) \
    case wm:                                 \
    {                                        \
        we.type = WindowEventType::Mouse;       \
        we.arg1 = GET_X_LPARAM(lParam);      \
        we.arg2 = GET_Y_LPARAM(lParam);      \
        we.arg3.int16Left = btn;            \
        we.arg3.int16Right = 0;             \
        push_event(we);                      \
        SetCapture(hwnd);                    \
        return 0;                            \
    }                                        \

#define check_button_up_case(wm, btn)   \
    case wm:                                 \
    {                                        \
        we.type = WindowEventType::Mouse;       \
        we.arg1 = GET_X_LPARAM(lParam);      \
        we.arg2 = GET_Y_LPARAM(lParam);      \
        we.arg3.int16Left = btn;            \
        we.arg3.int16Right = 1;             \
        push_event(we);                      \
        ReleaseCapture();                    \
        return 0;                            \
    }                                        \

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam)
{
    slxWindow* win = (slxWindow*)GetWindowLongPtrW(hwnd, 0);
    if (!win) return DefWindowProcW(hwnd, uMsg, wParam, lParam);
    windowEvent we{ };
    switch (uMsg)
    {
    case WM_USER_SLXCLOSE:
        DestroyWindow(hwnd);
        return 0;
    case WM_ERASEBKGND:
        return 0;
    case WM_CLOSE:
    {
        we.type = WindowEventType::Close;
        push_event(we);
        return 0;
    }
    case WM_MOVE:
    {
        int x = (int)(short)LOWORD(lParam);
        int y = (int)(short)HIWORD(lParam);
        we.type = WindowEventType::Move;
        we.arg1 = x;
        we.arg2 = y;
        push_event(we);
        return 0;
    }
    case WM_SIZE:
    {
        int width = (int)(short)LOWORD(lParam);
        int height = (int)(short)HIWORD(lParam);
        we.type = WindowEventType::Resize;
        we.arg1 = width;
        we.arg2 = height;
        push_event(we);
        return 0;
    }
    case WM_SYSKEYUP:
    case WM_SYSKEYDOWN:
        DefWindowProcW(hwnd, uMsg, wParam, lParam); [[fallthrough]];
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
            we.type = WindowEventType::KeyDown;
            we.arg1 = (int32_t)key;
            push_event(we);
            we.type = WindowEventType::KeyUp;
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
        we.type = !isKeyReleased ? WindowEventType::KeyDown : WindowEventType::KeyUp;
        we.arg1 = (int32_t)key;
        push_event(we);

        return 0;
    }
    case WM_SETFOCUS:
    {
        we.type = WindowEventType::GotFocus;
        push_event(we);
        return 0;
    }
    case WM_KILLFOCUS:
    {
        we.type = WindowEventType::LostFocus;
        push_event(we);
        return 0;
    }

    check_button_down_case(WM_LBUTTONDOWN, 1);
    check_button_down_case(WM_RBUTTONDOWN, 2);
    check_button_down_case(WM_MBUTTONDOWN, 3);
    check_button_up_case(WM_LBUTTONUP, 1);
    check_button_up_case(WM_RBUTTONUP, 2);
    check_button_up_case(WM_MBUTTONUP, 3);

    case WM_MOUSEMOVE:
    {
        we.type = WindowEventType::Mouse;
        we.arg1 = GET_X_LPARAM(lParam);
        we.arg2 = GET_Y_LPARAM(lParam);
        we.arg3.int16Left = 0;
        we.arg3.int16Right = 2;
        push_event(we);
        return 0;
    }
    case WM_MOUSEWHEEL:
    {
        we.type = WindowEventType::MouseWheel;
        we.arg1 = (int)GET_WHEEL_DELTA_WPARAM(wParam);
        push_event(we);
        return 0;
    }
    }
    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
#undef push_e
}