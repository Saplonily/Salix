namespace Monosand.Win32;

internal class Win32GraphicsImplBase : IGraphicsImpl
{
    private Win32RenderContext? renderContext;
    internal Win32RenderContext RenderContext
    {
        get
        {
            EnsureState();
            return renderContext!;
        }
    }

    RenderContext IGraphicsImpl.RenderContext => RenderContext;

    internal Win32GraphicsImplBase(Win32RenderContext context)
        => renderContext = context;

    protected virtual void Dispose(bool disposing)
        => renderContext = null;

    ~Win32GraphicsImplBase()
        => renderContext!.Invoke(() => Dispose(false));

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(renderContext is null, this);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}