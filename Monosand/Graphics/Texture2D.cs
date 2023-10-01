namespace Monosand;

public sealed class Texture2D : GraphicsResource
{
    private readonly ITexture2DImpl impl;

    public Texture2D(int width, int height)
        => impl = Game.Instance.Platform.CreateTexture2DImpl(Game.Instance.RenderContext, width, height);

    [CLSCompliant(false)]
    public unsafe Texture2D(int width, int height, void* data)
        : this(width, height)
    {
        if (data != null)
            impl.SetData(width, height, data);
    }

    public Texture2D(int width, int height, ReadOnlySpan<byte> data) : this(width, height)
        => SetData(width, height, data);

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data)
        => impl.SetData(width, height, data);

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

    public override void Dispose()
        => impl.Dispose();

    internal ITexture2DImpl GetImpl() 
        => impl;
}