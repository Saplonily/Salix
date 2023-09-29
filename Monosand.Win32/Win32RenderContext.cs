namespace Monosand.Win32;

// TODO dispose impl
public class Win32RenderContext : RenderContext
{
    private readonly Dictionary<VertexDeclaration, IntPtr> vertexDeclarations;
    private IntPtr handle;

    internal Win32RenderContext(IntPtr handle)
    {
        vertexDeclarations = new();
        this.handle = handle;
    }

    internal override void SwapBuffers()
    {
        EnsureState();
        Interop.MsdgSwapBuffers(handle);
    }

    internal override void SetViewport(int x, int y, int width, int height)
    {
        EnsureState(); 
        Interop.MsdgViewport(handle, x, y, width, height);
    }

    public override void Clear(Color color)
    {
        EnsureState(); 
        Interop.MsdgClear(handle, color);
    }

    internal unsafe IntPtr SafeGetVertexType(VertexDeclaration vertexDeclaration)
    {
        EnsureState();

        ThrowHelper.ThrowIfNull(vertexDeclaration);
        if (!vertexDeclarations.TryGetValue(vertexDeclaration, out IntPtr vertexType))
        {
            fixed (VertexElementType* ptr = vertexDeclaration.Attributes)
            {
                vertexType = Interop.MsdgRegisterVertexType(handle, ptr, vertexDeclaration.Count);
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
        Interop.MsdgDrawPrimitives(handle, vertexType, primitiveType, vptr, length * sizeof(T), length);
    }

    public override void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType)
    {
        EnsureState();
        ThrowHelper.ThrowIfDisposed(buffer.Disposed, nameof(buffer));

        Win32VertexBufferImpl impl = (Win32VertexBufferImpl)buffer.impl!;
        Interop.MsdgDrawBufferPrimitives(handle, impl.bufferHandle, primitiveType, impl.verticesCount);
    }

    internal override void SetTexture(int index, Texture2DImpl texImpl)
    {
        EnsureState();
        ThrowHelper.ThrowIfNegative(index);

        Interop.MsdgSetTexture(handle, index, ((Win32Texture2DImpl)texImpl).texHandle);
    }

    internal IntPtr GetHandle()
    {
        EnsureState();
        return handle;
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(handle == IntPtr.Zero, this);
}