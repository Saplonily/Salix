namespace Saladim.Salix.Windows;

internal sealed class TextureImpl : ITexture2DImpl
{
    private readonly int width, height;
    internal IntPtr nativePtr;

    int ITexture2DImpl.Width => width;
    int ITexture2DImpl.Height => height;

    public TextureImpl(int width, int height)
    {
        nativePtr = Interop.SLX_CreateTexture(width, height);
        if (nativePtr == IntPtr.Zero)
            Interop.Throw();
    }

    unsafe void ITexture2DImpl.SetData( void* data, ImageFormat format)
    {
        if (Interop.SLX_SetTextureData(nativePtr, width, height, data, format))
            Interop.Throw();
    }

    void IDisposable.Dispose()
    {
        if (Interop.SLX_DeleteTexture(nativePtr))
            Interop.Throw();
        nativePtr = IntPtr.Zero;
    }
}
