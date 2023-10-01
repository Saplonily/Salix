namespace Monosand.Win32;

// TODO dispose impl
public sealed class Win32RenderContext : RenderContext
{
    private readonly Dictionary<VertexDeclaration, IntPtr> vertexDeclarations;
    private IntPtr winHandle;

    internal Win32RenderContext(IntPtr winHandle)
    {
        vertexDeclarations = new();
        this.winHandle = winHandle;
    }

    internal override void SwapBuffers()
    {
        EnsureState();
        Interop.MsdgSwapBuffers(winHandle);
    }

    internal override void SetViewport(int x, int y, int width, int height)
    {
        EnsureState();
        Interop.MsdgViewport(winHandle, x, y, width, height);
    }

    public override void Clear(Color color)
    {
        EnsureState();
        Interop.MsdgClear(winHandle, color);
    }

    internal unsafe IntPtr SafeGetVertexType(VertexDeclaration vertexDeclaration)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(vertexDeclaration);
        if (!vertexDeclarations.TryGetValue(vertexDeclaration, out IntPtr vertexType))
        {
            fixed (VertexElementType* ptr = vertexDeclaration.Attributes)
            {
                vertexType = Interop.MsdgRegisterVertexType(winHandle, ptr, vertexDeclaration.Count);
                vertexDeclarations.Add(vertexDeclaration, vertexType);
            }
        }
        return vertexType;
    }

    [CLSCompliant(false)]
    public override unsafe void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration,
        PrimitiveType primitiveType,
        T* vptr, int length
        )
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(vertexDeclaration);

        IntPtr vertexType = SafeGetVertexType(vertexDeclaration);
        Interop.MsdgDrawPrimitives(winHandle, vertexType, primitiveType, vptr, length * sizeof(T), length);
    }

    public override void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType)
    {
        EnsureState();

        var impl = (Win32VertexBufferImpl)buffer.impl;
        Interop.MsdgDrawBufferPrimitives(winHandle, impl.GetBufferHandle(), primitiveType, impl.GetVerticesCount());
    }

    internal override void SetTexture(int index, ITexture2DImpl texImpl)
    {
        EnsureState();
        ThrowHelper.ThrowIfNegative(index);

        Interop.MsdgSetTexture(winHandle, index, ((Win32Texture2DImpl)texImpl).texHandle);
    }

    internal IntPtr GetWinHandle()
    {
        EnsureState();
        return winHandle;
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(winHandle == IntPtr.Zero, this);

    public override void SetShader(Shader? shader)
    {
        if (shader is not null)
        {
            Win32ShaderImpl si = (Win32ShaderImpl)shader.GetImpl();
            Interop.MsdgSetShader(winHandle, si.GetShaderHandle());
        }
        else
        {
            Interop.MsdgSetShader(winHandle, IntPtr.Zero);
        }
    }
}