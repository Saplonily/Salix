namespace Monosand;

public abstract class GraphicsResource : IDisposable
{
    private RenderContext? renderContext;

    public RenderContext RenderContext { get { EnsureState(); return renderContext!; } }

    public bool IsDisposed => renderContext == null;

    protected GraphicsResource(RenderContext renderContext)
    {
        ThrowHelper.ThrowIfNull(renderContext);
        this.renderContext = renderContext;
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (renderContext != null)
        {
            Dispose(true);
            renderContext = null;
            GC.SuppressFinalize(this);
        }
    }

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(renderContext is null, this);

    ~GraphicsResource()
        => renderContext!.Invoke(() => Dispose(false));
}