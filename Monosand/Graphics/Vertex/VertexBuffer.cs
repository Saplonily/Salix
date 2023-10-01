namespace Monosand;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> : IDisposable where T : unmanaged
{
    internal IVertexBufferImpl impl;
    internal VertexDeclaration vertexDeclaration;

    public VertexDeclaration VertexDeclaration => vertexDeclaration;

    public VertexBuffer(VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw)
    {
        ThrowHelper.ThrowIfNull(vertexDeclaration);

        impl = Game.Instance.Platform.CreateVertexBufferImpl(Game.Instance.RenderContext, vertexDeclaration, dataUsage);
        this.vertexDeclaration = vertexDeclaration;
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    public void SetData(T[] array)
    {
        unsafe
        {
            fixed (T* ptr = array)
            {
                impl.SetData(ptr, array.Length);
            }
        }
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    public void SetData(ReadOnlySpan<T> span)
    {
        unsafe
        {
            fixed (T* ptr = span)
            {
                impl.SetData(ptr, span.Length);
            }
        }
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetData(T* ptr, int length)
        => impl.SetData(ptr, length);
    

    public void Dispose()
        => impl.Dispose();
}