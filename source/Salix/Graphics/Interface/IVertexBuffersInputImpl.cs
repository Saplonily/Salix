namespace Saladim.Salix;

internal interface IVertexBuffersInputImpl : IDisposable
{
    void ReplaceBinding(int index, VertexBuffer vertexBuffer, int offset);
}