﻿namespace Monosand;

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
        nativeHandle = Interop.MsdgCreateSampler(filter, wrap);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Interop.MsdgDeleteSampler(nativeHandle);
    }
}