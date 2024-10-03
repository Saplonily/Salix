using Saladim.Salix.Windows;

namespace Saladim.Salix.Windows;

internal sealed class IndexBufferImpl : IIndexBufferImpl
{
    internal IntPtr nativePtr;

    unsafe internal IndexBufferImpl(int size)
    {
        nativePtr = Interop.SLX_CreateIndexBuffer(size);
        if (nativePtr == IntPtr.Zero) Interop.Throw();
    }

    unsafe void IIndexBufferImpl.SetIndexData(ushort* data, int offset, int size)
    {
        if (Interop.SLX_SetIndexBufferData(nativePtr, data, offset, size))
            Interop.Throw();
    }

    void IDisposable.Dispose()
    {
        if (Interop.SLX_DeleteIndexBuffer(nativePtr))
            Interop.Throw();
        nativePtr = IntPtr.Zero;
    }
}