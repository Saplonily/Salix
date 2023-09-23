namespace Monosand;

internal unsafe abstract class VertexBufferImpl
{
    internal abstract void SetData<T>(T* data, int length) where T : unmanaged;
}