namespace Monosand;

internal interface IVertexBufferImpl : IDisposable
{
    bool Indexed { get; }
    unsafe void SetIndexData(ushort* data, int count);
    unsafe void SetData<T>(T* data, int count) where T : unmanaged;
}