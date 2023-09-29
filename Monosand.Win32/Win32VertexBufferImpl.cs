namespace Monosand.Win32;

internal unsafe class Win32VertexBufferImpl : VertexBufferImpl
{
    private IntPtr winHandle;
    private VertexBufferDataUsage dataUsage;
    internal IntPtr bufferHandle;
    internal int verticesCount;

    internal Win32VertexBufferImpl(Win32WinImpl winImpl, VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage)
    {
        this.dataUsage = dataUsage;
        IntPtr vtype = winImpl.GetRenderContext().SafeGetVertexType(vertexDeclaration);
        bufferHandle = Interop.MsdgCreateVertexBuffer(winHandle = winImpl.GetHandle(), vtype);
    }

    internal override void SetData<T>(T* data, int length)
    {
        verticesCount = length;
        Interop.MsdgSetVertexBufferData(winHandle, bufferHandle, data, sizeof(T) * length, dataUsage);
    }

    internal override void Dispose()
    {
        if (winHandle != IntPtr.Zero)
        {
            Interop.MsdgDeleteVertexBuffer(winHandle, bufferHandle);
            winHandle = IntPtr.Zero;
            bufferHandle = IntPtr.Zero;
            dataUsage = (VertexBufferDataUsage)(-1);
            verticesCount = -1;
        }
    }
}