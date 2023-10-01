namespace Monosand;

internal interface IVertexBufferImpl : IDisposable
{
    unsafe void SetData<T>(T* data, int length) where T : unmanaged;
}