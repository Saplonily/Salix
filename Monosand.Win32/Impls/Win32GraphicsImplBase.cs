namespace Monosand.Win32;

internal class Win32GraphicsImplBase : IGraphicsImpl
{
    private Win32RenderContext? renderContext;

    internal Win32RenderContext RenderContext { get { EnsureState(); return renderContext!; } }

    RenderContext IGraphicsImpl.RenderContext => RenderContext;
    bool IGraphicsImpl.IsDisposed => renderContext == null;

    internal Win32GraphicsImplBase(Win32RenderContext context)
        => renderContext = context;

    protected virtual void Dispose(bool disposing)
    { 
    }

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(renderContext is null, this);

    public void Dispose()
    {
        if (renderContext != null)
        {
            renderContext = null;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    ~Win32GraphicsImplBase()
        => renderContext!.Invoke(() => Dispose(false));
}