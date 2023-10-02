namespace Monosand.Win32;

internal class GraphicsImplBase : IDisposable
{
    protected IntPtr winHandle;

    internal GraphicsImplBase(IntPtr winHandle)
    {
        this.winHandle = winHandle;
    }

    protected virtual void Dispose(bool disposing)
        => winHandle = IntPtr.Zero;

    ~GraphicsImplBase()
    {
        // gc is usually running on another thread
        // so make sure that the disposing method is running on main thread
        var pf = ((Win32Platform)Game.Instance.Platform);
        if (Thread.CurrentThread.ManagedThreadId == pf.MainThreadId)
        {
            Dispose(false);
        }
        else
        {
            lock (pf.queuedActions)
            {
                pf.queuedActions.Add(() => Dispose(false));
            }
        }
    }

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(winHandle == IntPtr.Zero, this);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}