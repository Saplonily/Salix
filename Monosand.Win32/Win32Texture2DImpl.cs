namespace Monosand.Win32;

internal sealed class Win32Texture2DImpl : Win32GraphicsImplBase, ITexture2DImpl
{
    private IntPtr handle;
    private int width, height;
    internal IntPtr Handle { get { EnsureState(); return handle; } }

    public int Width { get { EnsureState(); return width; } }
    public int Height { get { EnsureState(); return height; } }

    internal Win32Texture2DImpl(Win32RenderContext context, int width, int height)
        : base(context)
    {
        (this.width, this.height) = (width, height);
        handle = Interop.MsdgCreateTexture(width, height);
    }

    unsafe void ITexture2DImpl.SetData(int width, int height, void* data, ImageFormat format)
    {
        (this.width, this.height) = (width, height);
        Interop.MsdgSetTextureData(handle, width, height, data, format);
    }

    public void SetFilter(TextureFilterType filter)
    {
        EnsureState();
        Interop.MsdgSetTextureFilter(handle, filter, filter);
    }

    public void SetWrap(TextureWrapType wrap)
    {
        EnsureState();
        Interop.MsdgSetTextureWrap(handle, wrap);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (handle != IntPtr.Zero)
        {
            Interop.MsdgDeleteTexture(handle);
            handle = IntPtr.Zero;
        }
    }
}