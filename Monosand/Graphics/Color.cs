using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Monosand;

/// <summary>Represents a RGBA (Red, Green, Blue, Alpha) color. 4bytes per component.</summary>
[DebuggerDisplay("RGBA: <{R}, {G}, {B}, {A}>")]
[StructLayout(LayoutKind.Sequential)]
public partial struct Color : IEquatable<Color>
{
    /// <summary>Red component.</summary>
    public byte R;
    /// <summary>Green component.</summary>
    public byte G;
    /// <summary>Blue component.</summary>
    public byte B;
    /// <summary>Alpha component.</summary>
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

    public static bool operator ==(Color left, Color right)
        => left.Equals(right);

    public static bool operator !=(Color left, Color right)
        => !(left == right);

    public readonly override bool Equals(object? obj)
        => obj is Color color && Equals(color);

    public readonly bool Equals(Color other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public readonly override int GetHashCode()
        => HashCode.Combine(R, G, B, A);

    public readonly Vector4 ToVector4()
        => new(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
}