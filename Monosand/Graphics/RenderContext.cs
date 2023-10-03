namespace Monosand;
#pragma warning disable CS3011

// TODO dispose impl
public abstract class RenderContext
{
    protected Shader? currentShader;

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

    /// <summary>Draw <strong>indexed</strong> primitives with <see cref="VertexBuffer{T}"/> on this RenderContext.</summary>
    public abstract void DrawIndexedPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType) where T : unmanaged;

    internal abstract void SetTexture(int index, ITexture2DImpl texImpl);

    public abstract void SetShader(Shader? shader);

    public Shader? GetCurrentShader()
        => currentShader;

    public void SetTexture(int index, Texture2D texture2D)
        => SetTexture(index, texture2D.GetImpl());


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