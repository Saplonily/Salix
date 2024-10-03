using System.Diagnostics.CodeAnalysis;

namespace Saladim.Salix;

public abstract class GraphicsResource : IResource, IDisposable
{
    private RenderContext? renderContext;

    public RenderContext RenderContext { get { EnsureState(); return renderContext; } }

    public bool IsDisposed => renderContext == null;

    protected GraphicsResource(RenderContext renderContext)
    {
        ThrowHelper.ThrowIfNull(renderContext);
        this.renderContext = renderContext;
    }

    public void Dispose()
    {
        if (renderContext == null)
            return;
        try
        {
            Dispose(true);
        }
        finally
        {
            renderContext = null;
            GC.SuppressFinalize(this);
        }
    }

    protected virtual void Dispose(bool disposing)
        => renderContext!.OnResourceDisposed(this);

    [MemberNotNull(nameof(renderContext))]
    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(renderContext is null, this);

    // TODO don't use lambda capture
    ~GraphicsResource()
        => renderContext!.Invoke(() => Dispose(false));
}