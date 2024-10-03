namespace Saladim.Salix;

internal interface IRenderContextImpl : IDisposable
{
    bool VSyncEnabled { set; }

    double VSyncFrameTime { get; }

    IRenderTargetImpl? RenderTarget { get; set; }

    IShaderImpl? Shader { get; set; }

    void Viewport(int x, int y, int width, int height);

    void Clear(Color color);

    void DrawPrimitives(
        PrimitiveType primitiveType,
        ISingleVertexBufferInputImpl buffer,
        int start, int count
        );

    void DrawIndexedPrimitives(
        PrimitiveType primitiveType,
        ISingleVertexBufferInputImpl buffer,
        IIndexBufferImpl indexBuffer,
        int start, int count
        );

    void DrawPrimitives(
        PrimitiveType primitiveType,
        IVertexBuffersInputImpl vertexBuffersInput,
        int start, int count
        );

    void DrawIndexedPrimitives(
        PrimitiveType primitiveType,
        IVertexBuffersInputImpl vertexBuffersInput,
        IIndexBufferImpl indexBuffer,
        int start, int count
        );

    IVertexBufferImpl CreateVertexBufferImpl(int size);

    IIndexBufferImpl CreateIndexBufferImpl(int size);

    IVertexBuffersInputImpl CreateVertexBuffersInputImpl(ReadOnlySpan<VertexBufferBinding> bindings);

    ISingleVertexBufferInputImpl CreateSingleVertexBufferInputImpl(VertexDeclaration vertexDeclaration);

    ITexture2DImpl CreateTexture2DImpl(int width, int height);

    IRenderTargetImpl CreateRenderTargetImpl(ITexture2DImpl tex, int width, int height);
}