using System.Diagnostics;
using System.Numerics;

namespace Monosand;

[DebuggerDisplay("Width: {Width}, Height: {Height}")]
public sealed class Texture2D : GraphicsResource
{
    internal ITexture2DImpl Impl { get; private set; }

    public int Width => Impl.Width;
    public int Height => Impl.Height;
    public Vector2 Size => new(Impl.Width, Impl.Height);

    public Texture2D(RenderContext renderContext, int width, int height) : base(renderContext)
        => Impl = renderContext.CreateTexture2DImpl(width, height);

    [CLSCompliant(false)]
    public unsafe Texture2D(RenderContext renderContext, int width, int height, void* data, ImageFormat format)
        : this(renderContext, width, height)
    {
        if (data != null)
            Impl.SetData(width, height, data, format);
    }

    public Texture2D(RenderContext renderContext, int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
        : this(renderContext, width, height)
        => SetData(width, height, data, format);

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data, ImageFormat format)
        => Impl.SetData(width, height, data, format);

    public void SetData(int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
    {
        unsafe
        {
            fixed (byte* ptr = data)
                SetData(width, height, ptr, format);
        }
    }

    public override void Dispose()
        => Impl.Dispose();
}