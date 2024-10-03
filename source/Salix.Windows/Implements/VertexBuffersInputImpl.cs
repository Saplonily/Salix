namespace Saladim.Salix.Windows;

// stackalloc in loops
#pragma warning disable CA2014

internal unsafe class VertexBuffersInputImpl : IVertexBuffersInputImpl
{
    internal IntPtr nativePtr;

    public VertexBuffersInputImpl(ReadOnlySpan<VertexBufferBinding> bindings)
    {
        NVertexBufferBinding* nbindings = stackalloc NVertexBufferBinding[bindings.Length];
        for (int i = 0; i < bindings.Length; i++)
        {
            VertexBufferBinding binding = bindings[i];
            VertexBuffer buffer = binding.VertexBuffer;
            VertexBufferImpl impl = ((VertexBufferImpl)buffer.Impl);
            VertexDeclaration dec = buffer.vertexDeclaration;
            VertexElementType* elements = stackalloc VertexElementType[dec.Count];
            dec.Elements.CopyTo(new Span<VertexElementType>(elements, dec.Count));
            nbindings[i] = new()
            {
                VertexBuffer = impl.nativePtr,
                ElementsCount = dec.Count,
                Types = elements,
                Offset = binding.Offset,
                InstanceFrequency = binding.InstanceFrequency
            };
        }
        nativePtr = Interop.SLX_CreateVertexBuffersInput(bindings.Length, nbindings);
        if (nativePtr == IntPtr.Zero)
            Interop.Throw();
    }

    void IVertexBuffersInputImpl.ReplaceBinding(int index, VertexBuffer vertexBuffer, int offset)
    {
        IntPtr buf = ((VertexBufferImpl)vertexBuffer.Impl).nativePtr;
        if (Interop.SLX_ReplaceInputBuffer(nativePtr, index, buf, offset, vertexBuffer.vertexDeclaration.Size))
            Interop.Throw();
    }

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}
