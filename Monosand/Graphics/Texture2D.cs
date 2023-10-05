using System.Diagnostics;
using System.Numerics;

namespace Monosand;

[DebuggerDisplay("Width: {Width}, Height: {Height}")]
public sealed class Texture2D : GraphicsResource
{
    private readonly ITexture2DImpl impl;
    public int Width => impl.Width;
    public int Height => impl.Height;
    public Vector2 Size => new(impl.Width, impl.Height);

    public Texture2D(int width, int height)
        => impl = Game.Instance.Platform.CreateTexture2DImpl(Game.Instance.RenderContext, width, height);

    [CLSCompliant(false)]
    public unsafe Texture2D(int width, int height, void* data, ImageFormat format)
        : this(width, height)
    {
        if (data != null)
            impl.SetData(width, height, data, format);
    }

    public Texture2D(int width, int height, ReadOnlySpan<byte> data, ImageFormat format) : this(width, height)
        => SetData(width, height, data, format);

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data, ImageFormat format)
        => impl.SetData(width, height, data, format);

    public void SetData(int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
    {
        unsafe
        {
            fixed (byte* ptr = data)
            {
                SetData(width, height, ptr, format);
            }
        }
    }

    public override void Dispose()
        => impl.Dispose();

    internal ITexture2DImpl GetImpl()
        => impl;
}