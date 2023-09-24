using System.Runtime.InteropServices;

namespace Monosand;

/// <summary>Represents a RGBA (Red, Green, Blue, Alpha) color. 4bytes per component.</summary>
[StructLayout(LayoutKind.Sequential)]
public partial struct Color
{
    /// <summary>Red component</summary>
    public byte R;
    /// <summary>Green component</summary>
    public byte G;
    /// <summary>Blue component</summary>
    public byte B;
    /// <summary>Alpha component</summary>
    public byte A;

    /// <summary>
    /// Construct a color with RGBA with bytes.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    /// <param name="a">Alpha</param>
    public Color(byte r, byte g, byte b, byte a = byte.MaxValue)
        => (R, G, B, A) = (r, g, b, a);

    /// <summary>
    /// Construct a color with RGBA with floats.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    /// <param name="a">Alpha</param>
    public Color(float r, float g, float b, float a = 1.0f)
        => (R, G, B, A) = ((byte)(255f * r), (byte)(255f * g), (byte)(255f * b), (byte)(255f * a));

    public Color(Color color, byte alpha)
        => (R, G, B, A) = (color.R, color.G, color.B, alpha);

    public Color(Color color, float alpha)
        => (R, G, B, A) = (color.R, color.G, color.B, (byte)(255f * alpha));

    [CLSCompliant(false)]
    public Color(uint packedValue)
        => (R, G, B, A) = 
        ((byte)(packedValue >> 24), 
        (byte)((packedValue >> 16) & 0x00ffffff),
        (byte)((packedValue >> 8) & 0x0000ffff),
        (byte)packedValue);
    
    [CLSCompliant(false)]
    public static implicit operator Color(uint packedValue)
        => new(packedValue);
}