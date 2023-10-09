using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand.Win32;

internal sealed unsafe class Win32WinImpl : WinImpl
{
    private IntPtr handle;
    private Win32RenderContext? renderContext;

    public Win32WinImpl(int width, int height, string title, Window window)
    {
        IntPtr handle;
        fixed (char* ptitle = title)
        {
            handle = Interop.MsdCreateWindow(width, height, ptitle, (nint)GCHandle.Alloc(window, GCHandleType.Weak));
        }

        if (handle == IntPtr.Zero)
            throw new OperationFailedException("Can't create window.");

        this.handle = handle;
        renderContext = new(this.handle);
    }

    internal override void Destroy()
    {
        EnsureState();
        Interop.MsdDestroyWindow(handle);
        handle = IntPtr.Zero;
        renderContext = null;
    }

    internal override Point GetPosition()
    {
        EnsureState();
        Interop.RECT r = Interop.MsdGetWindowRect(handle);
        return new(r.left, r.top);
    }

    internal override Size GetSize()
    {
        EnsureState();
        Interop.RECT r = Interop.MsdGetWindowRect(handle);
        return new(r.right - r.left, r.bottom - r.top);
    }

    internal unsafe override void PollEvents()
    {
        EnsureState();
        ProcessQueuedActions();
        IntPtr winHandle = GetWinHandle();
        int count;
        int* e;
        void* handle = Interop.MsdBeginPollEvents(winHandle, out var ncount, out e);
        if (ncount > int.MaxValue)
            throw new OperationFailedException("Too many win events.(> int.MaxValue)");
        count = (int)ncount;
        int sizeInInt = 4 + sizeof(IntPtr) / 4;

        // magic number at ../Monosand.Win32.Native/win32_msg_loop.cpp :: event
        for (int i = 0; i < count * sizeInInt; i += sizeInInt)
        {
            IntPtr v = ((IntPtr*)(e + i + 4))[0];
            Window win = HandleToWin(v);
            switch (e[i])
            {
            case 1: if (win.OnClosing()) Interop.MsdDestroyWindow(GetWinHandle()); break;
            case 2: win.OnCallbackDestroy(); break;
            case 3: win.OnMoved(e[i + 1], e[i + 2]); break;
            case 4: win.OnResized(e[i + 1], e[i + 2]); break;
            case 5: win.OnKeyPressed((Key)e[i + 1]); break;
            case 6: win.OnKeyReleased((Key)e[i + 1]); break;
            case 7: win.OnGotFocus(); break;
            case 8: win.OnLostFocus(); break;
            case 9:
                int x = e[i + 1], y = e[i + 2];
                short btnType = *((short*)(e + i + 3) + 0);
                PointerButton button = (PointerButton)btnType;
                short downType = *((short*)(e + i + 3) + 1);
                if (downType == 0)
                    win.OnPointerPressed(button);
                else if (downType == 1)
                    win.OnPointerReleased(button);
                break;
            default: Debug.Fail("Unknown event type."); break;
            }
        }

        Interop.MsdEndPollEvents(winHandle, handle);

        static Window HandleToWin(IntPtr handle)
            => (Window)GCHandle.FromIntPtr(handle).Target!;
    }

    private static void ProcessQueuedActions()
    {
        var pf = (Win32Platform)Game.Instance.Platform;
        lock (pf.queuedActions)
        {
            foreach (var item in pf.queuedActions)
                item();
            pf.queuedActions.Clear();
        }
    }

    internal override void Show()
    {
        EnsureState();
        Interop.MsdShowWindow(handle);
    }

    internal override void Hide()
    {
        EnsureState();
        Interop.MsdHideWindow(handle);
    }

    internal override void SetPosition(int x, int y)
    {
        EnsureState();
        Interop.MsdSetWindowPos(handle, x, y);
    }

    internal override void SetSize(int width, int height)
    {
        EnsureState();
        Interop.MsdSetWindowSize(handle, width, height);
    }

    internal override RenderContext GetRenderContext()
    {
        EnsureState();
        return renderContext!;
    }

    internal IntPtr GetWinHandle()
    {
        EnsureState();
        return handle;
    }

    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(handle == IntPtr.Zero, this);
        ThrowHelper.ThrowIfDisposed(renderContext is null, this);
    }

    internal override void MainThreadDispose()
    {
        ProcessQueuedActions();
    }
}