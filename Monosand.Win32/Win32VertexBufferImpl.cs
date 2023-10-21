namespace Monosand.Win32;

internal sealed unsafe class Win32VertexBufferImpl : Win32GraphicsImplBase, IVertexBufferImpl
{
    private bool indexed;
    private VertexBufferDataUsage dataUsage;
    private IntPtr handle;
    private int verticesCount;
    private int indicesCount;
    bool IVertexBufferImpl.Indexed => indexed;

    internal int IndicesCount { get { EnsureState(); return indicesCount; } }
    internal int VerticesCount { get { EnsureState(); return verticesCount; } }
    internal IntPtr Handle { get { EnsureState(); return handle; } }

    internal Win32VertexBufferImpl(
        Win32RenderContext context,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage,
        bool indexed
        )
        : base(context)
    {
        this.dataUsage = dataUsage;
        this.indexed = indexed;
        indicesCount = -1;
        IntPtr vtype = context.SafeGetVertexType(vertexDeclaration);
        handle = Interop.MsdgCreateVertexBuffer(vtype, (byte)(indexed ? 1 : 0));
    }

    void IVertexBufferImpl.SetData<T>(T* data, int count)
    {
        EnsureState();
        verticesCount = count;
        Interop.MsdgSetVertexBufferData(handle, data, sizeof(T) * count, dataUsage);
    }

    void IVertexBufferImpl.SetIndexData(ushort* data, int count)
    {
        EnsureState();
        indicesCount = count;
        Interop.MsdgSetIndexBufferData(handle, data, sizeof(ushort) * count, dataUsage);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (handle != IntPtr.Zero)
        {
            Interop.MsdgDeleteVertexBuffer(handle);
            handle = IntPtr.Zero;
        }
    }
}