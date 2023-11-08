namespace Monosand;

public sealed class RenderTarget : GraphicsResource
{
    public int Width => Texture.Width;
    public int Height => Texture.Height;
    public Texture2D Texture { get; private set; }
    internal IRenderTargetImpl Impl { get; private set; }

    public unsafe RenderTarget(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        var tex = new Texture2D(renderContext, width, height);
        tex.SetData(width, height, (void*)0, ImageFormat.Rgba32);
        Impl = renderContext.CreateRenderTargetImpl(tex);
        Texture = tex;
    }

    public override void Dispose()
        => Impl.Dispose();
}