using System.Reflection.Metadata;

namespace Monosand.Win32;

public class Win32RenderContext : RenderContext
{
    private readonly Dictionary<VertexDeclaration, IntPtr> vertexDeclarations;
    internal IntPtr handle;
    private IntPtr Handle => handle == IntPtr.Zero ? throw new ObjectDisposedException(nameof(Win32WinImpl)) : handle;

    internal Win32RenderContext(IntPtr handle)
    {
        vertexDeclarations = new();
        this.handle = handle;
    }

    internal override void SwapBuffers()
    => Interop.MsdgSwapBuffers(Handle);

    internal override void SetViewport(int x, int y, int width, int height)
        => Interop.MsdgViewport(Handle, x, y, width, height);

    public override void Clear(Color color)
        => Interop.MsdgClear(Handle, color);

    internal unsafe IntPtr SafeGetVertexType(VertexDeclaration vertexDeclaration)
    {
        if (vertexDeclaration is null)
            throw new ArgumentNullException(nameof(vertexDeclaration));
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
        if (vertexDeclaration is null)
            throw new ArgumentNullException(nameof(vertexDeclaration));
        IntPtr vertexType = SafeGetVertexType(vertexDeclaration);
        Interop.MsdgDrawPrimitives(handle, vertexType, primitiveType, vptr, length * sizeof(T), length);
    }

    public override void DrawPrimitives<T>(VertexBuffer<T> buffer, PrimitiveType primitiveType)
    {
        if (buffer.Disposed)
            throw new ObjectDisposedException(nameof(buffer));
        Win32VertexBufferImpl impl = (Win32VertexBufferImpl)buffer.impl!;
        Interop.MsdgDrawBufferPrimitives(handle, impl.bufferHandle, primitiveType, impl.verticesCount);
    }
}