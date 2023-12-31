using System.Drawing;

namespace Monosand;

// TODO dispose impl
public sealed class RenderContext
{
    private readonly Dictionary<VertexDeclaration, IntPtr> vertexDeclarations;
    private readonly List<Action> queuedActions;
    private readonly int creationThreadId;
    private readonly IntPtr nativeHandle;
    private readonly double vSyncFrameTime = 0d;
    private Shader? currentShader;
    private RenderTarget? currentRenderTarget = null;
    private Rectangle viewport;
    private bool vSyncEnabled = false;

    private long totalDrawCalls;
    private Size windowSize;

    internal IntPtr NativeHandle => nativeHandle;

    public long TotalDrawCalls => totalDrawCalls;

    public Rectangle Viewport
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

    /// <summary>Indicates is this <see cref="RenderContext"/> enabled the Vertical Synchronization.</summary>
    public bool VSyncEnabled
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

    /// <summary>The frame time will be when the <see cref="VSyncEnabled"/> is true.</summary>
    public double VSyncFrameTime
    {
        get
        {
            EnsureState();
            return vSyncFrameTime;
        }
    }

    public RenderTarget? RenderTarget
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
                Viewport = new(0, 0, windowSize.Width, windowSize.Height);
            }
            else
            {
                Interop.MsdgSetRenderTarget(value.NativeHandle);
                Viewport = new(0, 0, value.Width, value.Height);
            }
            currentRenderTarget = value;
        }
    }

    public Shader? Shader
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
                Interop.MsdgSetShader(value.NativeHandle);
            else
                Interop.MsdgSetShader(IntPtr.Zero);
            currentShader = value;
        }
    }

    internal event Action? ViewportChanged;
    internal event Action? PreviewViewportChanged;
    internal event Action? PreviewRenderTargetChanged;

    public RenderContext()
    {
        vertexDeclarations = new();
        queuedActions = new(8);
        creationThreadId = Environment.CurrentManagedThreadId;
        vSyncFrameTime = Interop.MsdgGetVSyncFrameTime();
        nativeHandle = Interop.MsdCreateRenderContext();
        if (nativeHandle == IntPtr.Zero)
            throw new OperationFailedException("RenderContext creation failed.");
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

    internal void OnWindowResized(int width, int height)
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

    /// <summary>Draw primitives with <typeparamref name="T"/>* on this RenderContext.</summary>
    public unsafe void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration,
        PrimitiveType primitiveType,
        ReadOnlySpan<T> vertices
        ) where T : unmanaged
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(vertexDeclaration);
        totalDrawCalls++;

        IntPtr vertexType = SafeGetVertexType(vertexDeclaration);
        fixed (T* vptr = vertices)
            Interop.MsdgDrawPrimitives(vertexType, primitiveType, vptr, vertices.Length * sizeof(T), vertices.Length);
    }

    /// <summary>Draw primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        ThrowHelper.ThrowIfInvalid(buffer.Indexed, "This buffer is indexed.");
        totalDrawCalls++;

        Interop.MsdgDrawBufferPrimitives(buffer.NativeHandle, primitiveType, buffer.VerticesCount);
    }

    /// <summary>Draw <strong>indexed</strong> primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public void DrawIndexedPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(buffer);
        ThrowHelper.ThrowIfInvalid(!buffer.Indexed, "This buffer isn't indexed.");
        totalDrawCalls++;

        Interop.MsdgDrawIndexedBufferPrimitives(buffer.NativeHandle, primitiveType, buffer.IndicesCount);
    }

    public void SetTexture(int index, Texture2D texture)
    {
        EnsureState();
        ThrowHelper.ThrowIfNegative(index);

        Interop.MsdgSetTexture(index, texture.NativeHandle);
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
}