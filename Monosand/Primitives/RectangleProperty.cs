using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
public struct RectangleProperty<TProperty> where TProperty : unmanaged
{
    public TProperty TopLeft;
    public TProperty TopRight;
    public TProperty BottomLeft;
    public TProperty BottomRight;

    public RectangleProperty(in TProperty topLeft, in TProperty topRight, in TProperty bottomLeft, in TProperty bottomRight)
        => (TopLeft, TopRight, BottomLeft, BottomRight) = (topLeft, topRight, bottomLeft, bottomRight);

    public RectangleProperty(in TProperty value)
        => TopLeft = TopRight = BottomLeft = BottomRight = value;

    public static implicit operator RectangleProperty<TProperty>(in TProperty property)
        => new(property);
}