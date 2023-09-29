using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand.Win32;

#pragma warning disable CS3016

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
        {
            // TODO error handling
            throw new NotImplementedException("TODO: error handling");
        }

        this.handle = handle;
        renderContext = new(this.handle);
    }

    internal static void InitMsgCallbacks()
    {
        // set our callbacks
        // table at ../Monosand.Win32.Native/win32_msg_loop.cpp
        Interop.MsdSetMsgCallback(0, (delegate* unmanaged[Stdcall]<IntPtr, byte>)&OnMsgClose);
        Interop.MsdSetMsgCallback(1, (delegate* unmanaged[Stdcall]<int, int, IntPtr, void>)&OnMsgResize);
        Interop.MsdSetMsgCallback(2, (delegate* unmanaged[Stdcall]<int, int, IntPtr, void>)&OnMsgMove);
        Interop.MsdSetMsgCallback(3, (delegate* unmanaged[Stdcall]<IntPtr, void>)&OnMsgDestroy);
    }

    #region native callbacks

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Window HandleToWin(IntPtr gcHandle)
        => (Window)(GCHandle.FromIntPtr(gcHandle).Target!);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static byte OnMsgClose(IntPtr gcHandle)
        => HandleToWin(gcHandle).OnClosing() ? (byte)1 : (byte)0;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static void OnMsgDestroy(IntPtr gcHandle)
        => HandleToWin(gcHandle).OnCallbackDestroy();

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static void OnMsgMove(int x, int y, IntPtr gcHandle)
        => HandleToWin(gcHandle).OnMoved(x, y);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static void OnMsgResize(int width, int height, IntPtr gcHandle)
        => HandleToWin(gcHandle).OnResize(width, height);

    #endregion

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

    internal override void PollEvents()
    { 
        EnsureState(); 
        Interop.MsdPollEvents(handle);
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

    internal override Win32RenderContext GetRenderContext()
    {
        EnsureState();
        return renderContext!;
    }

    internal IntPtr GetHandle()
    {
        EnsureState();
        return handle;
    }
    
    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(handle == IntPtr.Zero, this);
        ThrowHelper.ThrowIfDisposed(renderContext is null, this);
    }
}