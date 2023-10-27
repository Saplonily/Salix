using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
public struct RectangleProp<TProp>
{
    public TProp TopLeft;
    public TProp TopRight;
    public TProp BottomLeft;
    public TProp BottomRight;

    public RectangleProp(in TProp topLeft, in TProp topRight, in TProp bottomLeft, in TProp bottomRight)
        => (TopLeft, TopRight, BottomLeft, BottomRight) = (topLeft, topRight, bottomLeft, bottomRight);

    public RectangleProp(in TProp value)
        => TopLeft = TopRight = BottomLeft = BottomRight = value;

    public static implicit operator RectangleProp<TProp>(in TProp prop)
        => new(prop);
}