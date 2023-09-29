namespace Monosand;

public class VertexDeclaration : IEquatable<VertexDeclaration>
{
    private readonly int hash;
    private readonly VertexElementType[] attr;

    public int Count => Attributes.Length;
    public ReadOnlySpan<VertexElementType> Attributes => new(attr);

    public VertexDeclaration(params VertexElementType[] attributes)
    {
        attr = attributes;

        HashCode hc = new();
        foreach (var item in attr) hc.Add(item);
        hash = hc.ToHashCode();
    }

    public override int GetHashCode() => hash;

    public bool Equals(VertexDeclaration? other)
        => other is not null && attr.SequenceEqual(other.attr);

    public override bool Equals(object? other)
        => other is VertexDeclaration vd && attr.SequenceEqual(vd.attr);

    public static bool operator ==(VertexDeclaration? left, VertexDeclaration? right)
        => left is not null && left.Equals(right);

    public static bool operator !=(VertexDeclaration? left, VertexDeclaration? right)
        => !(left == right);
}