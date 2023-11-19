namespace Monosand;

public sealed class RenderTarget : GraphicsResource
{
    private IntPtr nativeHandle;

    public int Width => Texture.Width;
    public int Height => Texture.Height;
    public Texture2D Texture { get; private set; }
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    public unsafe RenderTarget(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        var tex = new Texture2D(renderContext, width, height);
        tex.SetData(width, height, (void*)0, ImageFormat.Rgba32);
        nativeHandle = Interop.MsdgCreateRenderTarget(tex.NativeHandle);
        Texture = tex;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Interop.MsdgDeleteRenderTarget(nativeHandle);
        nativeHandle = IntPtr.Zero;
    }
}