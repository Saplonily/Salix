namespace Saladim.Salix;

// TODO support int indices
internal interface IIndexBufferImpl : IDisposable
{
    unsafe void SetIndexData(ushort* data, int offset, int size);
}
