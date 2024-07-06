using System.Diagnostics;

namespace Saladim.Salix;

[DebuggerDisplay("{TopLeft}, {TopRight}, {BottomLeft}, {BottomRight}")]
public struct RectangleProperty<TProperty> : IEquatable<RectangleProperty<TProperty>>
{
    public TProperty TopLeft;
    public TProperty TopRight;
    public TProperty BottomLeft;
    public TProperty BottomRight;

    public RectangleProperty(TProperty topLeft, TProperty topRight, TProperty bottomLeft, TProperty bottomRight)
        => (TopLeft, TopRight, BottomLeft, BottomRight) = (topLeft, topRight, bottomLeft, bottomRight);

    public RectangleProperty(TProperty value)
        => TopLeft = TopRight = BottomLeft = BottomRight = value;

    public static implicit operator RectangleProperty<TProperty>(TProperty property)
        => new(property);

    public static bool operator ==(RectangleProperty<TProperty> left, RectangleProperty<TProperty> right)
        => left.Equals(right);

    public static bool operator !=(RectangleProperty<TProperty> left, RectangleProperty<TProperty> right)
        => !(left == right);

    public readonly override bool Equals(object? obj)
        => obj is RectangleProperty<TProperty> property && Equals(property);

    public readonly bool Equals(RectangleProperty<TProperty> other)
        => EqualityComparer<TProperty>.Default.Equals(TopLeft, other.TopLeft) &&
           EqualityComparer<TProperty>.Default.Equals(TopRight, other.TopRight) &&
           EqualityComparer<TProperty>.Default.Equals(BottomLeft, other.BottomLeft) &&
           EqualityComparer<TProperty>.Default.Equals(BottomRight, other.BottomRight);

    public readonly override int GetHashCode()
        => HashCode.Combine(TopLeft, TopRight, BottomLeft, BottomRight);
}