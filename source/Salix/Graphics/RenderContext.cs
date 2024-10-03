using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Saladim.Salix;

// TODO dispose impl
public sealed class RenderContext
{
    private IRenderContextImpl? impl;
    internal IRenderContextImpl Impl { get { EnsureState(); return impl; } }

    private int creationThreadId;
    private List<Action> queuedActions;
    private bool vSyncEnabled = false;
    private double vSyncFrameTime = 0d;
    private Shader? currentShader;
    private RenderTarget? currentRenderTarget = null;
    private Rectangle viewport;
    private Texture2D?[] textureSlots;
    private Sampler?[] samplerSlots;

    private long totalDrawCalls;
    private Size windowSize;

    public long TotalDrawCalls => totalDrawCalls;

    public Rectangle Viewport
    {
        get { EnsureState(); return viewport; }
        set
        {
            EnsureState();
            PreviewStateChanged?.Invoke(RenderContextState.Viewport);
            impl.Viewport(value.X, value.Y, value.Width, value.Height);
            viewport = value;
            StateChanged?.Invoke(RenderContextState.Viewport);
        }
    }

    /// <summary>Indicates weather this <see cref="RenderContext"/> is enabled the Vertical Synchronization.</summary>
    public bool VSyncEnabled
    {
        get { EnsureState(); return vSyncEnabled; }
        set { EnsureState(); impl.VSyncEnabled = value; vSyncEnabled = value; }
    }

    /// <summary>The frame time will be when the <see cref="VSyncEnabled"/> is true.</summary>
    public double VSyncFrameTime
    {
        get { EnsureState(); return vSyncFrameTime; }
    }

    public RenderTarget? RenderTarget
    {
        get { EnsureState(); return currentRenderTarget; }
        set
        {
            EnsureState();
            if (currentRenderTarget == value) return;
            if (value is null)
            {
                PreviewStateChanged?.Invoke(RenderContextState.RenderTarget);
                impl.RenderTarget = null;
                Viewport = new(0, 0, windowSize.Width, windowSize.Height);
            }
            else
            {
                ThrowHelper.ThrowIfDisposed(value.IsDisposed, value);
                PreviewStateChanged?.Invoke(RenderContextState.RenderTarget);
                impl.RenderTarget = value.Impl;
                Viewport = new(0, 0, value.Width, value.Height);
            }
            currentRenderTarget = value;
            StateChanged?.Invoke(RenderContextState.RenderTarget);
        }
    }

    public Shader? Shader
    {
        get { EnsureState(); return currentShader; }
        set
        {
            if (currentShader == value) return;
            PreviewStateChanged?.Invoke(RenderContextState.Shader);
            bool result = value is not null ?
                Interop.SLX_SetShader(value.NativeHandle) :
                Interop.SLX_SetShader(IntPtr.Zero);
            if (result) Interop.Throw();
            currentShader = value;
            StateChanged?.Invoke(RenderContextState.Shader);
        }
    }

    public event Action<RenderContextState>? StateChanged;
    public event Action<RenderContextState>? PreviewStateChanged;

    internal RenderContext(Platform platform, Window window)
    {
        impl = platform.CreateRenderContextImpl(window.Impl);
        queuedActions = new(8);
        creationThreadId = Environment.CurrentManagedThreadId;
        vSyncFrameTime = impl.VSyncFrameTime;

        int length = 8;
        textureSlots = new Texture2D[length];
        samplerSlots = new Sampler[length];
    }

    internal void ProcessQueuedActions()
    {
        lock (queuedActions)
        {
            foreach (var item in queuedActions)
                item();
            queuedActions.Clear();
        }
    }

    internal void OnWindowResized(Window? _, int width, int height)
    {
        windowSize = new(width, height);
        if (RenderTarget is null)
            Viewport = new(0, 0, width, height);
    }

    /// <summary>Invoke an action on the RenderContext creation thread.</summary>
    public void Invoke(Action action)
    {
        if (Environment.CurrentManagedThreadId != creationThreadId)
            lock (queuedActions)
                queuedActions.Add(action);
        else
            action();
    }

    /// <summary>Clear this RenderContext in a color.</summary>
    public void Clear(Color color)
    {
        EnsureState();
        impl.Clear(color);
    }

    internal ISingleVertexBufferInputImpl FetchSingleVBI(VertexDeclaration vertexDeclaration)
    {
        // TODO: dictionary
        EnsureState();
        return impl.CreateSingleVertexBufferInputImpl(vertexDeclaration);
    }

    /// <summary>Draw primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public void DrawPrimitives<T>(PrimitiveType primitiveType, VertexBuffer<T> buffer) where T : unmanaged
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        totalDrawCalls++;
        impl.DrawPrimitives(primitiveType, buffer.InputImpl, 0, buffer.VerticesCount);
    }

    /// <summary>Draw <strong>indexed</strong> primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public void DrawIndexedPrimitives<T>(PrimitiveType primitiveType, VertexBuffer<T> buffer, IndexBuffer indexBuffer) where T : unmanaged
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        totalDrawCalls++;
        impl.DrawIndexedPrimitives(primitiveType, buffer.InputImpl, indexBuffer.Impl, 0, indexBuffer.IndicesCount);
    }

    public void SetTexture(int index, Texture2D texture)
    {
        EnsureState();
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), SR.ValueCannotBeNegative);
        ThrowHelper.ThrowIfNull(texture);
        ThrowHelper.ThrowIfDisposed(texture.IsDisposed, texture);

        PreviewStateChanged?.Invoke(RenderContextState.Texture);
        textureSlots[index] = texture;
        if (Interop.SLX_SetTexture(index, texture.NativeHandle))
            Interop.Throw();
        StateChanged?.Invoke(RenderContextState.Texture);
    }

    public void SetSampler(int index, Sampler sampler)
    {
        EnsureState();
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), SR.ValueCannotBeNegative);
        ThrowHelper.ThrowIfNull(sampler);
        ThrowHelper.ThrowIfDisposed(sampler.IsDisposed, sampler);

        PreviewStateChanged?.Invoke(RenderContextState.Sampler);
        samplerSlots[index] = sampler;
        bool result = Interop.SLX_SetSampler(index, sampler.NativeHandle);
        if (result) Interop.Throw();
        StateChanged?.Invoke(RenderContextState.Sampler);
    }

    internal void OnResourceDisposed(GraphicsResource resource)
    {
        switch (resource)
        {
        case Texture2D texture: CleanSlots(textureSlots, texture); break;
        case Sampler sampler: CleanSlots(samplerSlots, sampler); break;
        }
    }

    internal static void CleanSlots<T>(T?[] array, T value) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (ReferenceEquals(array[i], value))
                array[i] = null;
        }
    }

    [DebuggerStepThrough]
    [MemberNotNull(nameof(impl))]
    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(impl is null, this);
}