namespace Monosand;
#pragma warning disable CS3011

public abstract class RenderContext
{
    /// <summary>Swap the buffers. (see: DoubleBuffered)</summary>
    internal abstract void SwapBuffers();

    /// <summary>Set the viewport of this RenderContext.</summary>
    internal abstract void SetViewport(int x, int y, int width, int height);

    /// <summary>Clear this RenderContext in a color.</summary>
    public abstract void Clear(Color color);

    /// <summary>Draw primitives with <typeparamref name="T"/>* on this RenderContext.</summary>
    [CLSCompliant(false)]
    public unsafe abstract void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration, PrimitiveType primitiveType,
        T* vptr, int length
        ) where T : unmanaged;

    /// <summary>Draw primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public abstract void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged;

    public abstract void SetTexture(int index, Texture2D tex);

    public void DrawPrimitives<T>(VertexDeclaration vertexDeclaration, PrimitiveType primitiveType, ReadOnlySpan<T> vertices) where T : unmanaged
    {
        unsafe
        {
            fixed (T* vptr = vertices)
            {
                DrawPrimitives(vertexDeclaration, primitiveType, vptr, vertices.Length);
            }
        }
    }


}