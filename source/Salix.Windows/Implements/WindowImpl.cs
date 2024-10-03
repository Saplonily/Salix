using System.Drawing;
using System.Runtime.InteropServices;

namespace Saladim.Salix.Windows;

internal unsafe sealed partial class WindowImpl : IWindowImpl
{
    internal IntPtr nativeHandle;

    internal WindowImpl(int width, int height, string title)
    {
        IntPtr winHandle;
        fixed (char* ptitle = title)
            winHandle = Interop.SLX_CreateWindow(width, height, ptitle);
        if (winHandle == IntPtr.Zero)
            Interop.Throw(SR.FailedToCreateWindow);

        nativeHandle = winHandle;
    }

    void IWindowImpl.Show()
        => Interop.SLX_ShowWindow(nativeHandle);

    void IWindowImpl.Hide()
        => Interop.SLX_HideWindow(nativeHandle);

    void IWindowImpl.Close()
        => ((IWindowImpl)this).Dispose();

    void IWindowImpl.SwapBuffers()
        => Interop.SLX_SwapBuffers(nativeHandle);

    Point IWindowImpl.Position
    {
        get
        {
            Interop.SLX_GetWindowRect(nativeHandle, out Interop.RECT rect);
            return new(rect.left, rect.top);
        }
        set
        {
            Interop.SLX_SetWindowPos(nativeHandle, value.X, value.Y);
        }
    }

    Size IWindowImpl.Size
    {
        get
        {
            Interop.SLX_GetWindowRect(nativeHandle, out Interop.RECT rect);
            return new(rect.right - rect.left, rect.bottom - rect.top);
        }
        set
        {
            Interop.SLX_SetWindowPos(nativeHandle, value.Width, value.Height);
        }
    }

    string IWindowImpl.Title
    {
        get
        {
            int len = Interop.SLX_GetWindowTitle(nativeHandle, null);
            if (len < 0) throw new FrameworkException(SR.FailedToGetWindowTitle);
            byte[] chars = ByteArrayPool.Shared.Rent(len * sizeof(char));
            string str;
            try
            {
                fixed (byte* pchar = chars)
                {
                    _ = Interop.SLX_GetWindowTitle(nativeHandle, (char*)pchar);
                    str = new string((char*)pchar, 0, len);
                }
            }
            finally
            {
                ByteArrayPool.Shared.Return(chars);
            }
            return str;
        }
        set
        {
            fixed (char* str = value)
                Interop.SLX_SetWindowTitle(nativeHandle, str);
        }
    }

    void IDisposable.Dispose()
    {
        if (nativeHandle == IntPtr.Zero) return;
        Interop.SLX_DestroyWindow(nativeHandle);
        nativeHandle = IntPtr.Zero;
    }
}
