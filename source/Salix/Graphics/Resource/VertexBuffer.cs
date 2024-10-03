namespace Saladim.Salix;

public abstract class VertexBuffer : GraphicsResource
{
    private protected IVertexBufferImpl? impl;
    internal IVertexBufferImpl Impl { get { EnsureState(); return impl!; } }

    internal protected readonly VertexDeclaration vertexDeclaration;
    internal protected readonly int verticesCount;

    private protected VertexBuffer(
        RenderContext renderContext, 
        VertexDeclaration vertexDeclaration,
        int verticesCount
        ) 
        : base(renderContext)
    {
        this.vertexDeclaration = vertexDeclaration;
        this.verticesCount = verticesCount;
        impl = renderContext.Impl.CreateVertexBufferImpl(verticesCount * vertexDeclaration.Count);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        impl!.Dispose();
        impl = null;
    }
}
