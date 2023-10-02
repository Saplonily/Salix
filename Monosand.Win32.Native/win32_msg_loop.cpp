#include "pch.h"
#include <vector>
#include "exports.h"

enum class event : int32_t
{
    close = 1,
    destroy,
    move,
    resize
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
        int16_t int16_left;
        int16_t int16_right;
    } arg3;
    void* gc_handle;
};

std::vector<win_event>* event_list;

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
        event_list->push_back(we);

        return 0;
    }
    case WM_DESTROY:
    {
        we.type = event::destroy;
        event_list->push_back(we);

        return 0;
    }
    case WM_MOVE:
    {
        int x = (int)(short)LOWORD(lParam);
        int y = (int)(short)HIWORD(lParam);

        we.type = event::move;
        we.arg1 = x;
        we.arg2 = y;
        event_list->push_back(we);

        return 0;
    }
    case WM_SIZE:
    {
        int width = (int)(short)LOWORD(lParam);
        int height = (int)(short)HIWORD(lParam);

        we.type = event::resize;
        we.arg1 = width;
        we.arg2 = height;
        event_list->push_back(we);

        return 0;
    }
    }
    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
}

void window_msg_loop_init()
{
    event_list = new std::vector<win_event>;
    event_list->reserve(5);
}

static bool began_polling = false;
extern "C"
{
    EXPORT void* CALLCONV MsdBeginPollEvents(whandle* whandle, size_t* count, win_event** events)
    {
        assert(began_polling == false);

        // TODO message merging
        MSG msg{};
        while (PeekMessageW(&msg, whandle->hwnd, 0, 0, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }

        std::vector<win_event>* pre_vector = event_list;
        event_list = new std::vector<win_event>;

        *count = pre_vector->size();
        *events = pre_vector->data();
        began_polling = true;
        return pre_vector;
    }

    EXPORT void CALLCONV MsdEndPollEvents(whandle* whandle, void* handle)
    {
        assert(began_polling == true);
        std::vector<win_event>* pre_vector = static_cast<std::vector<win_event>*>(handle);
        delete pre_vector;
        began_polling = false;
    }
}