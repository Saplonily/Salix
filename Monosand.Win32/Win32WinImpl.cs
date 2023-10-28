using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Monosand.Win32;

internal sealed unsafe class Win32WinImpl : WindowImpl
{
    private Game game;
    private IntPtr winHandle;
    internal IntPtr WinHandle { get { EnsureState(); return winHandle; } }

    internal override string Title
    {
        get
        {
            EnsureState();
            char* str = stackalloc char[256];
            Interop.MsdGetWindowTitle(winHandle, str);
            return new string(str);
        }
        set
        {
            EnsureState();
            fixed (char* str = value)
                Interop.MsdSetWindowTitle(winHandle, str);
        }
    }

    internal override Point Position
    {
        get
        {
            EnsureState();
            Interop.RECT r = Interop.MsdGetWindowRect(winHandle);
            return new(r.left, r.top);
        }
        set
        {
            EnsureState();
            Interop.MsdSetWindowPos(winHandle, value.X, value.Y);
        }
    }

    internal override Size Size
    {
        get
        {
            EnsureState();
            Interop.RECT r = Interop.MsdGetWindowRect(winHandle);
            return new(r.right - r.left, r.bottom - r.top);
        }
        set
        {
            EnsureState();
            Interop.MsdSetWindowSize(winHandle, value.Width, value.Height);
        }
    }

    internal Win32WinImpl(int width, int height, string title, Window window)
    {
        IntPtr winHandle;
        fixed (char* ptitle = title)
            winHandle = Interop.MsdCreateWindow(width, height, ptitle, (IntPtr)GCHandle.Alloc(window, GCHandleType.Weak));


        if (winHandle == IntPtr.Zero)
            throw new OperationFailedException("Can't create window.");

        this.winHandle = winHandle;
        game = window.Game;
    }

    internal override void Destroy()
    {
        EnsureState();
        Interop.MsdDestroyWindow(winHandle);
        winHandle = IntPtr.Zero;
    }

    internal unsafe override void PollEvents()
    {
        EnsureState();
        game.RenderContext.ProcessQueuedActions();
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
            case 1: if (win.OnClosing()) Interop.MsdDestroyWindow(winHandle); break;
            case 2: win.OnCallbackDestroy(); break;
            case 3: win.OnMoved(e[i + 1], e[i + 2]); break;
            case 4: win.OnResized(e[i + 1], e[i + 2]); break;
            case 5: win.OnKeyPressed((Key)e[i + 1]); break;
            case 6: win.OnKeyReleased((Key)e[i + 1]); break;
            case 7: win.OnGotFocus(); break;
            case 8: win.OnLostFocus(); break;
            case 9:
            {
                int x = e[i + 1], y = e[i + 2];
                short btnType = *((short*)(e + i + 3) + 0);
                PointerButton button = (PointerButton)btnType;
                short downType = *((short*)(e + i + 3) + 1);
                if (downType == 0)
                    win.OnPointerPressed(x, y, button);
                else if (downType == 1)
                    win.OnPointerReleased(x, y, button);
                else if (downType == 2 && button == PointerButton.None)
                    win.OnPointerMoved(x, y);
            }
            break;
            case 10:
            {
                int x = e[i + 1], y = e[i + 2];
                int delta = e[i + 3];
                win.OnPointerWheelMoved(x, y, delta);
            }
            break;
            default: Debug.Fail("Unknown event type."); break;
            }
        }

        Interop.MsdEndPollEvents(winHandle, handle);

        static Window HandleToWin(IntPtr handle)
            => (Window)GCHandle.FromIntPtr(handle).Target!;
    }

    internal override void Show()
    {
        EnsureState();
        Interop.MsdShowWindow(winHandle);
    }

    internal override void Hide()
    {
        EnsureState();
        Interop.MsdHideWindow(winHandle);
    }

    internal override void SwapBuffers()
    {
        EnsureState();
        Interop.MsdgSwapBuffers(winHandle);
    }

    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(winHandle == IntPtr.Zero, this);
    }
}