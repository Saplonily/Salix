using System.Diagnostics;

namespace Saladim.Salix.Windows;

internal unsafe sealed class RenderContextImpl : IRenderContextImpl
{
    private RenderTargetImpl? renderTarget;
    private ShaderImpl? shader;
    internal IntPtr nativeHandle;

    bool IRenderContextImpl.VSyncEnabled { set => Interop.SLX_SetVSyncEnabled(value); }

    double IRenderContextImpl.VSyncFrameTime { get => Interop.SLX_GetVSyncFrameTime(); }

    IRenderTargetImpl? IRenderContextImpl.RenderTarget
    {
        get
        {
            EnsureState();
            return renderTarget;
        }
        set
        {
            if (value is null)
            {
                if (Interop.SLX_SetRenderTarget(IntPtr.Zero))
                    Interop.Throw();
            }
            else
            {
                if (Interop.SLX_SetRenderTarget(((RenderTargetImpl)value).nativePtr))
                    Interop.Throw();
            }
        }
    }

    IShaderImpl? IRenderContextImpl.Shader
    {
        get
        {
            EnsureState();
            return shader;
        }
        set
        {
            // REMIND
        }
    }


    internal RenderContextImpl(IWindowImpl windowImpl)
    {
        var rc = Interop.SLX_CreateRenderContext(((WindowImpl)windowImpl).nativeHandle);
        if (rc == IntPtr.Zero)
            Interop.Throw(SR.FailedToCreateRenderContext);
        nativeHandle = rc;
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);

    void IRenderContextImpl.Viewport(int x, int y, int width, int height)
    {
        if (Interop.SLX_Viewport(x, y, width, height))
            Interop.Throw();
    }

    void IRenderContextImpl.Clear(Color color)
    {
        if (Interop.SLX_Clear(color.R, color.G, color.B, color.A))
            Interop.Throw();
    }

    void IRenderContextImpl.DrawPrimitives(PrimitiveType primitiveType, ISingleVertexBufferInputImpl buffer, int start, int count)
    {
        throw new NotImplementedException();
    }

    void IRenderContextImpl.DrawIndexedPrimitives(PrimitiveType primitiveType, ISingleVertexBufferInputImpl buffer, IIndexBufferImpl indexBuffer, int start, int count)
    {
        throw new NotImplementedException();
    }

    void IRenderContextImpl.DrawPrimitives(PrimitiveType primitiveType, IVertexBuffersInputImpl vertexBuffersInput, int start, int count)
    {
        throw new NotImplementedException();
    }

    void IRenderContextImpl.DrawIndexedPrimitives(PrimitiveType primitiveType, IVertexBuffersInputImpl vertexBuffersInput, IIndexBufferImpl indexBuffer, int start, int count)
    {
        throw new NotImplementedException();
    }

    IVertexBufferImpl IRenderContextImpl.CreateVertexBufferImpl(int size) => new VertexBufferImpl(size);

    IIndexBufferImpl IRenderContextImpl.CreateIndexBufferImpl(int size) => new IndexBufferImpl(size);

    IVertexBuffersInputImpl IRenderContextImpl.CreateVertexBuffersInputImpl(ReadOnlySpan<VertexBufferBinding> bindings)
        => new VertexBuffersInputImpl(bindings);

    ISingleVertexBufferInputImpl IRenderContextImpl.CreateSingleVertexBufferInputImpl(VertexDeclaration vertexDeclaration)
        => new Single

    ITexture2DImpl IRenderContextImpl.CreateTexture2DImpl(int width, int height)
    {
        throw new NotImplementedException();
    }

    IRenderTargetImpl IRenderContextImpl.CreateRenderTargetImpl(ITexture2DImpl tex, int width, int height)
    {
        throw new NotImplementedException();
    }

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}
