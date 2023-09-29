namespace Monosand;

public sealed class Texture2D : IDisposable
{
    private Texture2DImpl? impl;

    [CLSCompliant(false)]
    public unsafe Texture2D(int width, int height, void* data)
        : this(width, height)
    {
        if (data != null)
            impl!.SetData(width, height, data);
    }

    public Texture2D(int width, int height)
    {
        impl = Game.Platform.CreateTexture2DImpl(Game.WinImpl, width, height);
    }

    public Texture2D(int width, int height, ReadOnlySpan<byte> data)
        : this(width, height)
        => SetData(width, height, data);

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data)
    {
        EnsureState();
        impl!.SetData(width, height, data);
    }

    public void SetData(int width, int height, ReadOnlySpan<byte> data)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                SetData(width, height, ptr);
            }
        }
    }

    ~Texture2D() => Dispose();

    public void Dispose()
    {
        if (impl is not null)
        {
            impl.Dispose();
            impl = null;
            GC.SuppressFinalize(this);
        }
    }

    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(impl is null, this);
    }

    internal Texture2DImpl GetImpl()
    {
        EnsureState();
        return impl!;
    }
}