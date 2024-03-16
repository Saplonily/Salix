using System.Diagnostics;

namespace Monosand;

[DebuggerDisplay("{First}, {Second}, {Third}")]
public struct TriangleProperty<TProperty> : IEquatable<TriangleProperty<TProperty>>
{
    public TProperty First;
    public TProperty Second;
    public TProperty Third;

    public TriangleProperty(TProperty first, TProperty second, TProperty third)
        => (First, Second, Third) = (first, second, third);

    public TriangleProperty(TProperty value)
        => First = Second = Third = value;

    public static implicit operator TriangleProperty<TProperty>(TProperty property)
        => new(property);

    public static bool operator ==(TriangleProperty<TProperty> left, TriangleProperty<TProperty> right)
        => left.Equals(right);

    public static bool operator !=(TriangleProperty<TProperty> left, TriangleProperty<TProperty> right)
        => !(left == right);

    public readonly override bool Equals(object? obj)
        => obj is TriangleProperty<TProperty> property && Equals(property);

    public readonly bool Equals(TriangleProperty<TProperty> other)
        => EqualityComparer<TProperty>.Default.Equals(First, other.First) &&
           EqualityComparer<TProperty>.Default.Equals(Second, other.Second) &&
           EqualityComparer<TProperty>.Default.Equals(Third, other.Third);

    public readonly override int GetHashCode()
        => HashCode.Combine(First, Second, Third);
}