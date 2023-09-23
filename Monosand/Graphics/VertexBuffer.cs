using System;

namespace Monosand;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> where T : unmanaged
{
    internal readonly VertexBufferImpl impl;
    internal readonly VertexDeclaration vertexDeclaration;

    public VertexBuffer(VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw)
    {
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
    {
        impl.SetData(ptr, length);
    }

    // TODO dispose impl
}