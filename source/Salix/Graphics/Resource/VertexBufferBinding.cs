namespace Saladim.Salix;

public struct VertexBufferBinding
{
    public VertexBuffer VertexBuffer { get; set; }

    public int Offset { get; set; }

    public int InstanceFrequency { get; set; }

    public VertexBufferBinding(VertexBuffer vertexBuffer, int offset, int instanceFrequency)
    {
        VertexBuffer = vertexBuffer;
        Offset = offset;
        InstanceFrequency = instanceFrequency;
    }

    public VertexBufferBinding(VertexBuffer vertexBuffer)
        : this(vertexBuffer, 0, 0)
    {
    }
}