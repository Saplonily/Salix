using System.Numerics;

namespace Saladim.Salix;

public sealed class VertexDeclaration : IEquatable<VertexDeclaration>
{
    private readonly int hash;
    private readonly VertexElementType[] elements;

    public int Count => Elements.Length;
    public ReadOnlySpan<VertexElementType> Elements => new(elements);
    public int Size { get; private set; }

    public VertexDeclaration(params ReadOnlySpan<VertexElementType> elementsSpan)
    {
        elements = elementsSpan.ToArray();
        Size = 0;
        foreach (var type in elements)
            Size += GetElementTypeSize(type);

        HashCode hc = new();
        foreach (var item in elements) hc.Add(item);
        hash = hc.ToHashCode();
    }

    public VertexDeclaration(params VertexElementType[] elements)
        : this(elementsSpan: elements)
    {
    }

    public VertexDeclaration(VertexDeclaration vertexDeclaration)
    {
        elements = vertexDeclaration.elements;
        hash = vertexDeclaration.hash;
        Size = vertexDeclaration.Size;
    }

    public override int GetHashCode() => hash;

    public bool Equals(VertexDeclaration? other)
        => other is not null && elements.SequenceEqual(other.elements);

    public override bool Equals(object? other)
        => other is VertexDeclaration vd && elements.SequenceEqual(vd.elements);

    public static bool operator ==(VertexDeclaration? left, VertexDeclaration? right)
        => left is not null && left.Equals(right);

    public static bool operator !=(VertexDeclaration? left, VertexDeclaration? right)
        => !(left == right);

#pragma warning disable IDE0049
    public unsafe static int GetElementTypeSize(VertexElementType vertexElementType) => vertexElementType switch
    {
        VertexElementType.Single => sizeof(Single),
        VertexElementType.Vector2 => sizeof(Vector2),
        VertexElementType.Vector3 => sizeof(Vector3),
        VertexElementType.Vector4 => sizeof(Vector4),
        VertexElementType.Color => sizeof(Color),
        _ => throw new ArgumentOutOfRangeException(nameof(vertexElementType), vertexElementType, SR.EnumOutOfRange)
    };
#pragma warning restore
}