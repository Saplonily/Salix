namespace Monosand.Win32;

internal sealed class Win32Texture2DImpl : GraphicsImplBase, ITexture2DImpl
{
    internal IntPtr texHandle;
    internal int width, height;

    internal Win32Texture2DImpl(Win32RenderContext context, int width, int height)
        : base(context.GetWinHandle())
    {
        (this.width, this.height) = (width, height);
        texHandle = Interop.MsdgCreateTexture(winHandle, width, height);
    }

    unsafe void ITexture2DImpl.SetData(int width, int height, void* data)
    {
        (this.width, this.height) = (width, height);
        Interop.MsdgSetTextureData(winHandle, texHandle, width, height, data);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (texHandle != IntPtr.Zero)
        {
            texHandle = IntPtr.Zero;
            // TODO delete texture in opengl side
            throw new NotImplementedException();
        }
    }
}