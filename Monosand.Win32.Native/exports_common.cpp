#include "pch.h"
#include "thirdparty/glad/glad_wgl.h"
#include "exports.h"
#include "whandle.h"

const wchar_t* Monosand = L"Monosand";
extern void* MsgCallbacks[];

void window_gl_init();
LRESULT CALLBACK WindowProc(_In_ HWND hwnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);

// WndExtra:
// GCHandle of the managed Monosand.Win32.Win32WinImpl

static PIXELFORMATDESCRIPTOR pfd;
extern "C"
{
    EXPORT int CALLCONV MsdInit()
    {
        pfd = {
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

        WNDCLASSW wc{};
        wc.lpfnWndProc = WindowProc;
        wc.hCursor = LoadCursorW(nullptr, IDC_ARROW);
        wc.hbrBackground = (HBRUSH)(COLOR_WINDOW);
        wc.lpszClassName = Monosand;
        wc.cbWndExtra = sizeof(void*) * 1;
        RegisterClassW(&wc);

        return 0;
    }

    EXPORT whandle* CALLCONV MsdCreateWindow(int width, int height, wchar_t* title, void* gc_handle)
    {
        HWND hwnd = CreateWindowExW(NULL, Monosand, title, WS_OVERLAPPEDWINDOW,
            CW_USEDEFAULT, CW_USEDEFAULT,
            width, height,
            NULL, NULL, NULL, NULL
        );
        ShowWindow(hwnd, SW_HIDE);
        UpdateWindow(hwnd);

        // TODO impl error handler
        HDC hdc = GetDC(hwnd);
        int pixelFormat = ChoosePixelFormat(hdc, &pfd);
        SetPixelFormat(hdc, pixelFormat, &pfd);
        int i = 0;
        HGLRC hglrc = wglCreateContext(hdc);
        wglMakeCurrent(hdc, hglrc);
        gladLoadGL();
        gladLoadWGL(hdc);
        wglMakeCurrent(nullptr, nullptr);
        wglDeleteContext(hglrc);

        GLint attribs[] = {
            WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
            WGL_CONTEXT_MINOR_VERSION_ARB, 3,
            WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
        #if _DEBUG
            WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_DEBUG_BIT_ARB,
        #endif
            0
        };

        hglrc = wglCreateContextAttribsARB(hdc, nullptr, attribs);
        assert(hglrc != nullptr);
        wglMakeCurrent(hdc, hglrc);

        window_gl_init();

        SetWindowLongPtrW(hwnd, 0, (LONG_PTR)gc_handle);
        // we'll delete it at MsdDestroyWindow
        whandle* handle = new whandle;
        handle->hwnd = hwnd;
        handle->hdc = hdc;
        handle->hglrc = hglrc;
        return handle;
    }

    EXPORT void CALLCONV MsdPollEvents(whandle* handle)
    {
        MSG msg{};
        while (PeekMessageW(&msg, handle->hwnd, NULL, NULL, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
    }

    EXPORT void CALLCONV MsdShowWindow(whandle* handle) { ShowWindow(handle->hwnd, SW_NORMAL); }

    EXPORT void CALLCONV MsdHideWindow(whandle* handle) { ShowWindow(handle->hwnd, SW_HIDE); }

    EXPORT void CALLCONV MsdSetMsgCallback(int index, void* callback) { MsgCallbacks[index] = callback; }

    EXPORT void CALLCONV MsdDestroyWindow(whandle* handle)
    {
        DestroyWindow(handle->hwnd);
        // make sure our window has received and handled WM_DESTORY
        MsdPollEvents(handle);
        delete handle;
    }

    EXPORT RECT CALLCONV MsdGetWindowRect(whandle* handle)
    {
        RECT rect{};
        GetWindowRect(handle->hwnd, &rect);
        return rect;
    }

    EXPORT void CALLCONV MsdSetWindowSize(whandle* handle, int width, int height)
    {
        SetWindowPos(handle->hwnd, NULL, 0, 0, width, height, SWP_NOMOVE);
    }

    EXPORT void CALLCONV MsdSetWindowPos(whandle* handle, int x, int y)
    {
        SetWindowPos(handle->hwnd, NULL, x, y, 0, 0, SWP_NOSIZE);
    }
}