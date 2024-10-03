using System.Diagnostics;
using System.Numerics;

namespace Saladim.Salix;

[DebuggerDisplay("Width: {Width}, Height: {Height}")]
public sealed class Texture2D : GraphicsResource
{
    private ITexture2DImpl? impl;
    internal ITexture2DImpl Impl { get { EnsureState(); return impl!; } }

    private readonly int width, height;

    public int Width { get { EnsureState(); return width; } }
    public int Height { get { EnsureState(); return height; } }
    public Vector2 Size { get { EnsureState(); return new(width, height); } }
    public Vector2 Center => Size / 2.0f;

    public Texture2D(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        (this.width, this.height) = (width, height);
        impl = renderContext.Impl.CreateTexture2DImpl(width, height);
        //nativeHandle = Interop.SLX_CreateTexture(width, height);
        //if (nativeHandle == IntPtr.Zero) Interop.Throw();
    }

    public unsafe void SetData(ReadOnlySpan<byte> data, ImageFormat format)
    {
        EnsureState();
        if (data.Length != width * height)
            // REMIND
            throw new ArgumentOutOfRangeException();
        fixed (byte* ptr = data)
            impl!.SetData(ptr, format);
    }

    [CLSCompliant(false)]
    public unsafe void SetData(void* data, ImageFormat format)
    {
        EnsureState();
        impl!.SetData(data, format);
        //if (Interop.SLX_SetTextureData(nativeHandle, width, height, data, format))
        //    Interop.Throw();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        impl!.Dispose();
        impl = null;
        //if (Interop.SLX_DeleteTexture(nativeHandle))
        //    Interop.Throw();
        //nativeHandle = IntPtr.Zero;
    }
}