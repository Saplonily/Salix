namespace Saladim.Salix;

internal interface IVertexBufferImpl : IDisposable
{
    unsafe void SetData(void* data, int offset, int size);
}