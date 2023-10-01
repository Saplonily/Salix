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
        => Dispose(false);

    protected void EnsureState()
        => ThrowHelper.ThrowIfDisposed(winHandle == IntPtr.Zero, this);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}