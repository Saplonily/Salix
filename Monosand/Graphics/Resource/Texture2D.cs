using System.Diagnostics;
using System.Numerics;

namespace Monosand;

[DebuggerDisplay("Width: {Width}, Height: {Height}")]
public sealed class Texture2D : GraphicsResource
{
    private TextureFilterType filter;
    private TextureWrapType wrap;

    internal ITexture2DImpl Impl { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public TextureFilterType Filter { get => filter; set { filter = value; Impl.SetFilter(filter); } }
    public TextureWrapType Wrap { get => wrap; set { wrap = value; Impl.SetWrap(wrap); } }
    public Vector2 Size => new(Width, Height);
    public Vector2 Center => Size / 2.0f;

    public Texture2D(RenderContext renderContext, int width, int height) : base(renderContext)
    {
        (Width, Height) = (width, height);
        Impl = renderContext.CreateTexture2DImpl(width, height);
        Filter = TextureFilterType.Linear;
        Wrap = TextureWrapType.ClampToEdge;
    }

    public Texture2D(RenderContext renderContext, int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
        : this(renderContext, width, height)
        => SetData(width, height, data, format);

    [CLSCompliant(false)]
    public unsafe Texture2D(RenderContext renderContext, int width, int height, void* data, ImageFormat format)
        : this(renderContext, width, height)
    {
        if (data != null)
        {
            (Width, Height) = (width, height);
            Impl.SetData(width, height, data, format);
        }
    }

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data, ImageFormat format)
    {
        (Width, Height) = (width, height);
        Impl.SetData(width, height, data, format);
    }

    public unsafe void SetData(int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
    {
        fixed (byte* ptr = data)
            SetData(width, height, ptr, format);
    }

    public override void Dispose()
        => Impl.Dispose();
}