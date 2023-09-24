namespace Monosand.Win32;

internal unsafe class Win32VertexBufferImpl : VertexBufferImpl
{
    private IntPtr winHandle;
    internal IntPtr bufferHandle;
    private readonly VertexBufferDataUsage dataUsage;
    internal int verticesCount;

    internal Win32VertexBufferImpl(WinImpl winImpl, VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage)
    {
        this.dataUsage = dataUsage;
        IntPtr vtype = ((Win32WinImpl)winImpl).SafeGetVertexType(vertexDeclaration);
        bufferHandle = Interop.MsdgCreateVertexBuffer(winHandle = ((Win32WinImpl)winImpl).Handle, vtype);
    }

    internal override void SetData<T>(T* data, int length)
    {
        verticesCount = length;
        Interop.MsdgSetVertexBufferData(winHandle, bufferHandle, data, sizeof(T) * length, dataUsage);
    }

    internal override void Dispose()
    {
        Interop.MsdgDeleteVertexBuffer(winHandle, bufferHandle);
        winHandle = IntPtr.Zero;
        bufferHandle = IntPtr.Zero;
        verticesCount = -1;
    }
}