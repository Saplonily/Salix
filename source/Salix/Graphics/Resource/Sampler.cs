namespace Salix;

// TODO lazy create
// TODO dispose check
public sealed class Sampler : GraphicsResource
{
    private TextureFilterType filterType;
    private TextureWrapType wrapType;
    private IntPtr nativeHandle;

    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    public TextureFilterType FilterType { get { EnsureState(); return filterType; } }
    public TextureWrapType WrapType { get { EnsureState(); return wrapType; } }

    public Sampler(RenderContext renderContext, TextureFilterType filter, TextureWrapType wrap)
        : base(renderContext)
    {
        (filterType, wrapType) = (filter, wrap);
        nativeHandle = Interop.SLX_CreateSampler(filter, wrap);
        if (nativeHandle == IntPtr.Zero) Interop.Throw();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Interop.SLX_DeleteSampler(nativeHandle))
            Interop.Throw();
    }
}