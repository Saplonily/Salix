namespace Monosand;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> : IDisposable where T : unmanaged
{
    public bool Indexed => impl.Indexed;

    internal IVertexBufferImpl impl;
    internal VertexDeclaration vertexDeclaration;

    public VertexDeclaration VertexDeclaration => vertexDeclaration;

    public VertexBuffer(
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw,
        bool indexed = false
        )
    {
        ThrowHelper.ThrowIfNull(vertexDeclaration);

        impl = Game.Instance.Platform.CreateVertexBufferImpl(Game.Instance.RenderContext, vertexDeclaration, dataUsage, indexed);
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

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    [CLSCompliant(false)]
    public void SetIndexData(ushort[] array)
    {
        unsafe
        {
            fixed (ushort* ptr = array)
            {
                impl.SetIndexData(ptr, array.Length);
            }
        }
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    [CLSCompliant(false)]
    public void SetIndexData(ReadOnlySpan<ushort> span)
    {
        unsafe
        {
            fixed (ushort* ptr = span)
            {
                impl.SetIndexData(ptr, span.Length);
            }
        }
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetData(T* ptr, int length)
        => impl.SetData(ptr, length);

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort* ptr, int length)
        => impl.SetIndexData(ptr, length);


    public void Dispose()
        => impl.Dispose();
}