namespace Saladim.Salix;

public sealed class RenderTarget : GraphicsResource
{
    private IRenderTargetImpl? impl;
    internal IRenderTargetImpl Impl { get { EnsureState(); return impl!; } }

    public int Width => Texture.Width;
    public int Height => Texture.Height;
    public Texture2D Texture { get; private set; }

    public unsafe RenderTarget(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        Texture = new Texture2D(renderContext, width, height);
        impl = renderContext.Impl.CreateRenderTargetImpl(Texture.Impl,width, height);
        //var tex = new Texture2D(renderContext, width, height);
        //tex.SetData(width, height, (void*)0, ImageFormat.Rgba32);
        //nativeHandle = Interop.SLX_CreateRenderTarget(tex.NativeHandle);
        //if (nativeHandle == IntPtr.Zero) Interop.Throw();
        //Texture = tex;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        impl!.Dispose();
        impl = null;
        //if (Interop.SLX_DeleteRenderTarget(nativeHandle))
        //    Interop.Throw();
        //nativeHandle = IntPtr.Zero;
    }
}