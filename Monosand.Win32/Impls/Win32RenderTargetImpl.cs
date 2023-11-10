namespace Monosand.Win32;

// TODO dispose impl
internal class Win32RenderTargetImpl : Win32GraphicsImplBase, IRenderTargetImpl
{
    private IntPtr handle;
    internal IntPtr Handle { get { EnsureState(); return handle; } }

    internal Win32RenderTargetImpl(Win32RenderContext renderContext, Win32Texture2DImpl impl)
        : base(renderContext)
    {
        handle = Interop.MsdgCreateRenderTarget(impl.Handle);
    }
}