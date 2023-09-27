namespace Monosand;

public class VertexDeclaration : IEquatable<VertexDeclaration>
{
    private int? hash;
    private VertexElementType[] attr;

    public int Count => Attributes.Length;
    public ReadOnlySpan<VertexElementType> Attributes { get => new(attr); }

    public VertexDeclaration(params VertexElementType[] attributes)
        => attr = attributes;

    public bool Equals(VertexDeclaration? other)
        => other is not null && attr.SequenceEqual(other.attr);

    public override bool Equals(object? other)
        => other is VertexDeclaration vd && attr.SequenceEqual(vd.attr);

    public override int GetHashCode()
    {
        if (hash is not null)
            return hash.Value;
        HashCode hc = new();
        foreach (var item in attr)
            hc.Add(item);
        int result = hc.ToHashCode();
        hash = result;
        return result;
    }

    public static bool operator ==(VertexDeclaration? left, VertexDeclaration? right)
        => left is not null && left.Equals(right);

    public static bool operator !=(VertexDeclaration? left, VertexDeclaration? right)
        => !(left == right);
}