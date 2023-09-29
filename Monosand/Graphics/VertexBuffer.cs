namespace Monosand;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> : IDisposable where T : unmanaged
{
    internal VertexBufferImpl? impl;
    internal VertexDeclaration? vertexDeclaration;

    public bool Disposed => impl is null;
    public VertexDeclaration VertexDeclaration => vertexDeclaration ?? throw new ObjectDisposedException(nameof(VertexBuffer<T>));

    public VertexBuffer(VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw)
    {
        if (vertexDeclaration is null)
            throw new ArgumentNullException(nameof(vertexDeclaration));

        impl = Game.Platform.CreateVertexBufferImpl(Game.WinImpl, vertexDeclaration, dataUsage);
        this.vertexDeclaration = vertexDeclaration;
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    public void SetData(T[] array)
    {
        unsafe
        {
            fixed (T* ptr = array)
            {
                impl!.SetData(ptr, array.Length);
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
                impl!.SetData(ptr, span.Length);
            }
        }
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetData(T* ptr, int length)
    {
        impl!.SetData(ptr, length);
    }

    ~VertexBuffer() => Dispose();

    public void Dispose()
    {
        if (impl is not null)
        {
            impl.Dispose();
            impl = null;
            GC.SuppressFinalize(this);
        }
    }
}