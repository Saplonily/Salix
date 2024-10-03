namespace Saladim.Salix.Windows;

internal sealed class RenderTargetImpl : IRenderTargetImpl
{
    internal IntPtr nativePtr;

    public RenderTargetImpl(TextureImpl textureImpl)
    {
        nativePtr = Interop.SLX_CreateRenderTarget(textureImpl.nativePtr);
    }

    void IDisposable.Dispose()
    {
        if (Interop.SLX_DeleteRenderTarget(nativePtr))
            Interop.Throw();
        nativePtr = IntPtr.Zero;
    }
}
