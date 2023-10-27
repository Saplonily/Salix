using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
public struct RectangleProp<TProp>
{
    public TProp TopLeft;
    public TProp TopRight;
    public TProp BottomLeft;
    public TProp BottomRight;

    public RectangleProp(TProp topLeft, TProp topRight, TProp bottomLeft, TProp bottomRight)
        => (TopLeft, TopRight, BottomLeft, BottomRight) = (topLeft, topRight, bottomLeft, bottomRight);

    public RectangleProp(TProp value)
        => TopLeft = TopRight = BottomLeft = BottomRight = value;
}