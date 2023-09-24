namespace Monosand;

public class VertexDeclaration
{
    public int Count => Attributes.Length;

    public VertexElementType[] Attributes { get; }

    public VertexDeclaration(params VertexElementType[] attributes)
        => Attributes = attributes;
}