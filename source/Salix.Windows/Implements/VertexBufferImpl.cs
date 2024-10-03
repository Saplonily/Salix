using Saladim.Salix.Windows;

namespace Saladim.Salix.Windows;

internal sealed class VertexBufferImpl : IVertexBufferImpl
{
    internal IntPtr nativePtr;

    unsafe internal VertexBufferImpl(int size)
    {
        nativePtr = Interop.SLX_CreateVertexBuffer(size);
        if (nativePtr == IntPtr.Zero) Interop.Throw();
    }

    unsafe void IVertexBufferImpl.SetData(void* data, int offset, int size)
    {
        if (Interop.SLX_SetVertexBufferData(nativePtr, data, offset, size))
            Interop.Throw();
    }

    void IDisposable.Dispose()
    {
        if (Interop.SLX_DeleteVertexBuffer(nativePtr))
            Interop.Throw();
        nativePtr = IntPtr.Zero;
    }
}