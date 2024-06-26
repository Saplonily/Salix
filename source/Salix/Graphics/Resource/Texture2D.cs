using System.Diagnostics;
using System.Numerics;

namespace Salix;

[DebuggerDisplay("Width: {Width}, Height: {Height}")]
public sealed class Texture2D : GraphicsResource
{
    private IntPtr nativeHandle;
    private int width, height;
    private TextureFilterType filter;
    private TextureWrapType wrap;
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    public int Width { get { EnsureState(); return width; } }
    public int Height { get { EnsureState(); return height; } }
    public Vector2 Size => new(Width, Height);
    public Vector2 Center => Size / 2.0f;

    public TextureFilterType Filter
    {
        get { EnsureState(); return filter; }
        set
        {
            EnsureState();
            if (Interop.SLX_SetTextureFilter(nativeHandle, filter, filter))
                Interop.Throw();
            filter = value;
        }
    }

    public TextureWrapType Wrap
    {
        get { EnsureState(); return wrap; }
        set
        {
            EnsureState();
            if (Interop.SLX_SetTextureWrap(nativeHandle, wrap))
                Interop.Throw();
            wrap = value;
        }
    }

    public Texture2D(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        (this.width, this.height) = (width, height);
        nativeHandle = Interop.SLX_CreateTexture(width, height);
        if (nativeHandle == IntPtr.Zero) Interop.Throw();
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
        if (data == null) return;
        SetData(width, height, data, format);
    }

    public unsafe void SetData(int width, int height, ReadOnlySpan<byte> data, ImageFormat format)
    {
        fixed (byte* ptr = data)
            SetData(width, height, ptr, format);
    }

    [CLSCompliant(false)]
    public unsafe void SetData(int width, int height, void* data, ImageFormat format)
    {
        EnsureState();
        (this.width, this.height) = (width, height);
        if (Interop.SLX_SetTextureData(nativeHandle, width, height, data, format))
            Interop.Throw();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Interop.SLX_DeleteTexture(nativeHandle))
            Interop.Throw();
        nativeHandle = IntPtr.Zero;
    }
}