﻿namespace Saladim.Salix;

public sealed class RenderTarget : GraphicsResource
{
    private IntPtr nativeHandle;

    public int Width => Texture.Width;
    public int Height => Texture.Height;
    public Texture2D Texture { get; private set; }
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    public unsafe RenderTarget(RenderContext renderContext, int width, int height)
        : base(renderContext)
    {
        var tex = new Texture2D(renderContext, width, height);
        tex.SetData(width, height, (void*)0, ImageFormat.Rgba32);
        nativeHandle = Interop.SLX_CreateRenderTarget(tex.NativeHandle);
        if (nativeHandle == IntPtr.Zero) Interop.Throw();
        Texture = tex;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Interop.SLX_DeleteRenderTarget(nativeHandle)) 
            Interop.Throw();
        nativeHandle = IntPtr.Zero;
    }
}