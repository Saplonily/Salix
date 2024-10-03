namespace Saladim.Salix;

/// <summary>
/// A type of Buffer used to store vertices data.
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged.</typeparam>
public sealed unsafe class VertexBuffer<T> : VertexBuffer where T : unmanaged
{
    private ISingleVertexBufferInputImpl? inputImpl;
    internal ISingleVertexBufferInputImpl InputImpl { get { EnsureState(); return inputImpl!; } }

    public VertexDeclaration VertexDeclaration { get { EnsureState(); return vertexDeclaration; } }
    public int VerticesCount { get { EnsureState(); return verticesCount; } }

    public unsafe VertexBuffer(
        RenderContext renderContext,
        VertexDeclaration vertexDeclaration,
        int verticesCount
        ) : base(renderContext, vertexDeclaration, verticesCount)
    {
        ThrowHelper.ThrowIfNull(vertexDeclaration);
        // TODO lazy init?
        inputImpl = renderContext.FetchSingleVBI(vertexDeclaration);
    }

    public unsafe void SetData(T[] array, int offset)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(array);
        fixed (T* data = array)
            impl!.SetData(data, offset * sizeof(T), array.Length * sizeof(T));
    }

    public unsafe void SetData(ReadOnlySpan<T> span, int offset)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(span, SR.VerticesDataIsNull);
        fixed (T* data = span)
            impl!.SetData(data, offset * sizeof(T), span.Length * sizeof(T));
    }

    [CLSCompliant(false)]
    public unsafe void SetData(T* data, int offset, int count)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(data, SR.VerticesDataIsNull);
        impl!.SetData(data, offset * sizeof(T), count * sizeof(T));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        inputImpl!.Dispose();
        inputImpl = null;
    }
}