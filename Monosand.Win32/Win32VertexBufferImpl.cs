namespace Monosand.Win32;

internal sealed unsafe class Win32VertexBufferImpl : GraphicsImplBase, IVertexBufferImpl
{
    private bool indexed;
    private VertexBufferDataUsage dataUsage;
    private IntPtr bufferHandle;
    private int verticesCount;
    bool IVertexBufferImpl.Indexed => indexed;

    internal Win32VertexBufferImpl(
        Win32RenderContext context,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage,
        bool indexed
        )
        : base(context.GetWinHandle())
    {
        this.dataUsage = dataUsage;
        this.indexed = indexed;
        IntPtr vtype = context.SafeGetVertexType(vertexDeclaration);
        bufferHandle = Interop.MsdgCreateVertexBuffer(winHandle, vtype, (byte)(indexed ? 1 : 0));
    }

    void IVertexBufferImpl.SetData<T>(T* data, int count)
    {
        verticesCount = count;
        Interop.MsdgSetVertexBufferData(winHandle, bufferHandle, data, sizeof(T) * count, dataUsage);
    }

    void IVertexBufferImpl.SetIndexData(ushort* data, int count)
    {
        verticesCount = count;
        Interop.MsdgSetIndexBufferData(winHandle, bufferHandle, data, sizeof(ushort) * count, dataUsage);
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