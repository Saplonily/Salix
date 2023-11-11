using System.Drawing;

namespace Monosand.Win32;

// TODO dispose impl
public sealed class Win32RenderContext : RenderContext
{
    private readonly IntPtr rcHandle;
    private readonly double vSyncFrameTime = 0d;

    private readonly Dictionary<VertexDeclaration, IntPtr> vertexDeclarations;
    private Shader? currentShader;
    private RenderTarget? currentRenderTarget = null;
    private Rectangle viewport;
    private bool vSyncEnabled = false;

    internal override event Action? ViewportChanged;
    internal override event Action? PreviewViewportChanged;
    internal override event Action? PreviewRenderTargetChanged;

    public override Shader? Shader
    {
        get
        {
            EnsureState();
            return currentShader;
        }
        set
        {
            if (value == currentShader) return;
            if (value is not null)
            {
                var si = (Win32ShaderImpl)value.Impl;
                Interop.MsdgSetShader(si.Handle);
            }
            else
            {
                Interop.MsdgSetShader(IntPtr.Zero);
            }
            currentShader = value;
        }
    }

    public override Rectangle Viewport
    {
        get
        {
            EnsureState();
            return viewport;
        }
        set
        {
            EnsureState();
            PreviewViewportChanged?.Invoke();
            Interop.MsdgViewport(value.X, value.Y, value.Width, value.Height);
            viewport = value;
            ViewportChanged?.Invoke();
        }
    }

    public override bool VSyncEnabled
    {
        get
        {
            EnsureState();
            return vSyncEnabled;
        }
        set
        {
            EnsureState();
            Interop.MsdgSetVSyncEnabled(value ? (byte)1 : (byte)0);
            vSyncEnabled = value;
        }
    }

    public override double VSyncFrameTime
    {
        get
        {
            EnsureState();
            return vSyncFrameTime;
        }
    }

    public override RenderTarget? RenderTarget
    {
        get
        {
            EnsureState();
            return currentRenderTarget;
        }
        set
        {
            PreviewRenderTargetChanged?.Invoke();
            if (value is null)
            {
                Interop.MsdgSetRenderTarget(IntPtr.Zero);
                currentRenderTarget = value;
                Viewport = new(0, 0, windowSize.Width, windowSize.Height);
            }
            else
            {
                Interop.MsdgSetRenderTarget(((Win32RenderTargetImpl)value.Impl).Handle);
                currentRenderTarget = value;
                Viewport = new(0, 0, value.Width, value.Height);
            }
        }
    }

    internal Win32RenderContext()
    {
        vertexDeclarations = new();
        rcHandle = Interop.MsdCreateRenderContext();

        vSyncFrameTime = Interop.MsdgGetVSyncFrameTime();
    }

    public override void Clear(Color color)
    {
        EnsureState();
        Interop.MsdgClear(color);
    }

    internal unsafe IntPtr SafeGetVertexType(VertexDeclaration vertexDeclaration)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(vertexDeclaration);
        if (!vertexDeclarations.TryGetValue(vertexDeclaration, out IntPtr vertexType))
        {
            fixed (VertexElementType* ptr = vertexDeclaration.Attributes)
            {
                vertexType = Interop.MsdgRegisterVertexType(ptr, vertexDeclaration.Count);
                vertexDeclarations.Add(vertexDeclaration, vertexType);
            }
        }
        return vertexType;
    }

    public override unsafe void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration,
        PrimitiveType primitiveType,
        ReadOnlySpan<T> vertices
        )
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(vertexDeclaration);
        drawcalls++;

        IntPtr vertexType = SafeGetVertexType(vertexDeclaration);
        fixed (T* vptr = vertices)
            Interop.MsdgDrawPrimitives(vertexType, primitiveType, vptr, vertices.Length * sizeof(T), vertices.Length);
    }

    public override void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        ThrowHelper.ThrowIfInvalid(buffer.Indexed, "This buffer is indexed.");
        drawcalls++;

        var impl = (Win32VertexBufferImpl)buffer.Impl;
        Interop.MsdgDrawBufferPrimitives(impl.Handle, primitiveType, impl.VerticesCount);
    }

    public override void DrawIndexedPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        ThrowHelper.ThrowIfInvalid(!buffer.Indexed, "This buffer isn't indexed.");
        drawcalls++;

        var impl = (Win32VertexBufferImpl)buffer.Impl;
        Interop.MsdgDrawIndexedBufferPrimitives(impl.Handle, primitiveType, impl.IndicesCount);
    }

    public override void SetTexture(int index, Texture2D texture)
    {
        EnsureState();
        ThrowHelper.ThrowIfNegative(index);

        Interop.MsdgSetTexture(index, ((Win32Texture2DImpl)texture.Impl).Handle);
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(rcHandle == IntPtr.Zero, this);

    internal override IVertexBufferImpl CreateVertexBufferImpl(
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage,
        bool indexed
        )
        => new Win32VertexBufferImpl(this, vertexDeclaration, dataUsage, indexed);

    internal override ITexture2DImpl CreateTexture2DImpl(int width, int height)
        => new Win32Texture2DImpl(this, width, height);

    internal override unsafe IShaderImpl CreateGlslShaderImpl(ReadOnlySpan<byte> vertSource, ReadOnlySpan<byte> fragSource)
    {
        fixed (byte* vptr = vertSource)
        fixed (byte* fptr = fragSource)
            return Win32ShaderImpl.FromGlsl(this, vptr, fptr);
    }

    internal override IRenderTargetImpl CreateRenderTargetImpl(Texture2D texture2D)
        => new Win32RenderTargetImpl(this, (Win32Texture2DImpl)texture2D.Impl);

    internal void AttachTo(Window win)
    {
        var wh = ((Win32WinImpl)win.Impl).WinHandle;
        Interop.MsdAttachRenderContext(wh, rcHandle);
    }
}