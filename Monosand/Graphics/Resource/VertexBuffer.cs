namespace Monosand;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> : GraphicsResource where T : unmanaged
{
    internal IVertexBufferImpl Impl { get; private set; }
    private VertexDeclaration vertexDeclaration;

    public bool Indexed => Impl.Indexed;
    public VertexDeclaration VertexDeclaration => vertexDeclaration;

    public VertexBuffer(
        RenderContext renderContext,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw,
        bool indexed = false
        ) : base(renderContext)
    {
        ThrowHelper.ThrowIfNull(vertexDeclaration);

        Impl = renderContext.CreateVertexBufferImpl(vertexDeclaration, dataUsage, indexed);
        this.vertexDeclaration = vertexDeclaration;
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    public unsafe void SetData(T[] array)
    {
        ThrowHelper.ThrowIfNull(array);
        fixed (T* ptr = array)
            Impl.SetData(ptr, array.Length);
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    public unsafe void SetData(ReadOnlySpan<T> span)
    {
        ThrowHelper.ThrowIfInvalid(span.IsEmpty);
        fixed (T* ptr = span)
            Impl.SetData(ptr, span.Length);
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort[] array)
    {
        ThrowHelper.ThrowIfNull(array);
        fixed (ushort* ptr = array)
            Impl.SetIndexData(ptr, array.Length);
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ReadOnlySpan<ushort> span)
    {
        ThrowHelper.ThrowIfInvalid(span.IsEmpty);
        fixed (ushort* ptr = span)
            Impl.SetIndexData(ptr, span.Length);
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetData(T* ptr, int count)
    {
        ThrowHelper.ThrowIfInvalid(ptr is null);
        Impl.SetData(ptr, count);
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort* ptr, int count)
    {
        ThrowHelper.ThrowIfInvalid(ptr is null);
        Impl.SetIndexData(ptr, count);
    }

    public override void Dispose()
        => Impl.Dispose();
}