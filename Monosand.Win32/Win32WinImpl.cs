using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand.Win32;

#pragma warning disable CS3016

internal sealed unsafe class Win32WinImpl : WinImpl
{
    private readonly Dictionary<VertexDeclaration, uint> vertexDeclarations;
    private IntPtr handle;
    private IntPtr Handle => handle == IntPtr.Zero ? throw new ObjectDisposedException(nameof(Win32WinImpl)) : handle;

    public Win32WinImpl(int width, int height, string title, Window window)
    {
        IntPtr handle = IntPtr.Zero;
        fixed (char* ptitle = title)
        {
            handle = Interop.MsdCreateWindow(width, height, ptitle, (nint)GCHandle.Alloc(window, GCHandleType.Weak));
        }

        if (handle == IntPtr.Zero)
        {
            // TODO error handle
            throw new NotImplementedException("TODO: error handle");
        }

        this.handle = handle;
        vertexDeclarations = new();
    }

    internal static void InitMsgCallbacks()
    {
        // set our callbacks
        // table at ../Monosand.Win32.Native/msg_callbacks.cpp
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
        Interop.MsdDestroyWindow(Handle);
        handle = IntPtr.Zero;
    }
    internal override Point GetPosition()
    {
        Interop.RECT r = Interop.MsdGetWindowRect(Handle);
        return new(r.left, r.top);
    }

    internal override Size GetSize()
    {
        Interop.RECT r = Interop.MsdGetWindowRect(Handle);
        return new(r.right - r.left, r.bottom - r.top);
    }

    internal override void PollEvents()
        => Interop.MsdPollEvents(Handle);

    internal override void Show()
        => Interop.MsdShowWindow(Handle);

    internal override void Hide()
        => Interop.MsdHideWindow(Handle);

    internal override void SetPosition(int x, int y)
        => Interop.MsdSetWindowPos(Handle, x, y);

    internal override void SetSize(int width, int height)
        => Interop.MsdSetWindowSize(Handle, width, height);

    internal override void SwapBuffers()
        => Interop.MsdgSwapBuffers(Handle);

    internal override void SetViewport(int x, int y, int width, int height)
        => Interop.MsdgViewport(Handle, x, y, width, height);

    internal override void Clear(Color color)
        => Interop.MsdgClear(Handle, color);

    internal override unsafe void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration,
        PrimitiveType primitiveType,
        T[] vertices
        ) // note that where T : unmanged
    {
        uint vertexType;
        if (!vertexDeclarations.TryGetValue(vertexDeclaration, out vertexType))
        {
            fixed (VertexElementType* ptr = vertexDeclaration.Attributes)
            {
                vertexType = Interop.MsdgRegisterVertexType(handle, ptr, vertexDeclaration.Count);
                vertexDeclarations.Add(vertexDeclaration, vertexType);
            }
        }
        fixed (T* tptr = vertices)
        {
            Interop.MsdgDrawPrimitives(handle, vertexType, primitiveType, tptr, vertices.Length * sizeof(T), vertices.Length);
        }
    }
}