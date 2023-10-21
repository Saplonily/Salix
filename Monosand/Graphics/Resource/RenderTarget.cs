namespace Monosand;

public sealed class RenderTarget : GraphicsResource
{
    internal IRenderTargetImpl Impl { get; private set; }

    public RenderTarget(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        Impl = renderContext.CreateRenderTargetImpl(new Texture2D(renderContext, width, height));
    }

    public override void Dispose()
        => Impl.Dispose();
}