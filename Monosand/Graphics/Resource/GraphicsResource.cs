namespace Monosand;


public abstract class GraphicsResource : IDisposable
{
    public RenderContext RenderContext { get; private set; }

    public GraphicsResource(RenderContext renderContext)
        => RenderContext = renderContext;

    public abstract void Dispose();
}