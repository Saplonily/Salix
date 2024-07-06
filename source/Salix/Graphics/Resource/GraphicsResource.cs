namespace Saladim.Salix;

public abstract class GraphicsResource : IResource, IDisposable
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
        => RenderContext.OnResourceDisposed(this);

    public void Dispose()
    {
        if (renderContext == null)
            return;
        Dispose(true);
        renderContext = null;
        GC.SuppressFinalize(this);
    }

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(renderContext is null, this);

    ~GraphicsResource()
        => renderContext!.Invoke(() => Dispose(false));
}