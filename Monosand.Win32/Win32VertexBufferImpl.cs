namespace Monosand.Win32;

internal sealed unsafe class Win32VertexBufferImpl : GraphicsImplBase, IVertexBufferImpl
{
    private VertexBufferDataUsage dataUsage;
    private IntPtr bufferHandle;
    private int verticesCount;

    internal Win32VertexBufferImpl(Win32RenderContext context, VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage)
        : base(context.GetWinHandle())
    {
        this.dataUsage = dataUsage;
        IntPtr vtype = context.SafeGetVertexType(vertexDeclaration);
        bufferHandle = Interop.MsdgCreateVertexBuffer(winHandle, vtype);
    }

    void IVertexBufferImpl.SetData<T>(T* data, int length)
    {
        verticesCount = length;
        Interop.MsdgSetVertexBufferData(winHandle, bufferHandle, data, sizeof(T) * length, dataUsage);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (bufferHandle != IntPtr.Zero)
        {
            Interop.MsdgDeleteVertexBuffer(winHandle, bufferHandle);
            bufferHandle = IntPtr.Zero;
        }
    }

    internal IntPtr GetBufferHandle()
    {
        EnsureState();
        return bufferHandle;
    }

    internal int GetVerticesCount()
    {
        EnsureState();
        return verticesCount;
    }
}