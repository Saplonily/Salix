using System.Drawing;

namespace Monosand;

// TODO dispose impl
public abstract class RenderContext
{
    protected long drawcalls;
    protected Size windowSize;
    private List<Action> queuedActions;
    private int creationThreadId;
    public delegate void ViewportChangedEventHandler(RenderContext renderContext, Rectangle rectangle);
    public abstract event ViewportChangedEventHandler? ViewportChanged;
    public abstract event Action? PreviewViewportChanged;

    public abstract Shader? Shader { get; set; }
    public abstract RenderTarget? RenderTarget { get; set; }
    public abstract Rectangle Viewport { get; set; }

    /// <summary>Indicates is this <see cref="RenderContext"/> enabled the Vertical Synchronization.</summary>
    public abstract bool VSyncEnabled { get; set; }

    /// <summary>The frame time will be when the <see cref="VSyncEnabled"/> is true.</summary>
    public abstract double VSyncFrameTime { get; }
    public long TotalDrawCalls => drawcalls;

    public RenderContext()
    {
        queuedActions = new(4);
        creationThreadId = Environment.CurrentManagedThreadId;
    }

    /// <summary>Clear this RenderContext in a color.</summary>
    public abstract void Clear(Color color);

    /// <summary>Draw primitives with <typeparamref name="T"/>* on this RenderContext.</summary>
    public abstract void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration,
        PrimitiveType primitiveType,
        ReadOnlySpan<T> vertices
        ) where T : unmanaged;

    /// <summary>Draw primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public abstract void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged;

    /// <summary>Draw <strong>indexed</strong> primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public abstract void DrawIndexedPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged;

    public abstract void SetTexture(int index, Texture2D texture2D);

    internal abstract IVertexBufferImpl CreateVertexBufferImpl(
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage,
        bool indexed
        );

    internal abstract ITexture2DImpl CreateTexture2DImpl(int width, int height);

    internal abstract IShaderImpl CreateGlslShaderImpl(ReadOnlySpan<byte> vertSource, ReadOnlySpan<byte> fragSource);

    internal abstract IRenderTargetImpl CreateRenderTargetImpl(Texture2D texture2D);

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
}