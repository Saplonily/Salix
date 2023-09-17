#include "pch.h"

constexpr int cb_Close = 0;
constexpr int cb_Resize = 1;
constexpr int cb_Move = 2;
constexpr int cb_Destroy = 3;

void* MsgCallbacks[4];

LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam)
{
    void* gcHandle = (void*)GetWindowLongPtrW(hwnd, 0);
    switch (uMsg)
    {
    case WM_ERASEBKGND:
        return 1;

    case WM_CLOSE:
    {
        byte rt = static_cast<byte(*)(void*)>(MsgCallbacks[cb_Close])(gcHandle);
        if (rt) DestroyWindow(hwnd);
        return 0;
    }
    case WM_DESTROY:
    {
        static_cast<void(*)(void*)>(MsgCallbacks[cb_Destroy])(gcHandle);
        return 0;
    }
    case WM_MOVE:
    {
        // TODO check if it's maximized or minimized
        int x = (int)(short)LOWORD(lParam);
        int y = (int)(short)HIWORD(lParam);
        static_cast<void(*)(int, int, void*)>(MsgCallbacks[cb_Move])(x, y, gcHandle);
        return 0;
    }
    case WM_SIZE:
        // TODO check if it's maximized or minimized
        int width = (int)(short)LOWORD(lParam);
        int height = (int)(short)HIWORD(lParam);
        static_cast<void(*)(int, int, void*)>(MsgCallbacks[cb_Resize])(width, height, gcHandle);
        return 0;
    }
    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
}