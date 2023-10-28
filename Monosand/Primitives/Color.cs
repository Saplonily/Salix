using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand;

/// <summary>Represents a RGBA (Red, Green, Blue, Alpha) color. 4bytes per component.</summary>
[DebuggerDisplay("RGBA: ({R}, {G}, {B}, {A})")]
[StructLayout(LayoutKind.Sequential)]
public partial struct Color : IEquatable<Color>
{
    /// <summary>Red component.</summary>
    public float R;
    /// <summary>Green component.</summary>
    public float G;
    /// <summary>Blue component.</summary>
    public float B;
    /// <summary>Alpha component.</summary>
    public float A;

    /// <summary>
    /// Construct a color with RGBA in floats.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    /// <param name="a">Alpha</param>
    public Color(float r, float g, float b, float a = 1.0f)
        => (R, G, B, A) = (r, g, b, a);

    public Color(Color color, float alpha)
        => (R, G, B, A) = (color.R, color.G, color.B, alpha);

    public static bool operator ==(Color left, Color right)
        => left.Equals(right);

    public static bool operator !=(Color left, Color right)
        => !(left == right);

    public readonly override bool Equals(object? obj)
        => obj is Color color && Equals(color);

    // TODO Vector128 can be used
    public readonly bool Equals(Color other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public readonly override int GetHashCode()
        => HashCode.Combine(R, G, B, A);

    public readonly Vector4 ToVector4()
    { Color c = this; return Unsafe.As<Color, Vector4>(ref c); }

    public readonly void Deconstruct(out float r, out float g, out float b, out float a)
        => (r, g, b, a) = (R, G, B, A);

    public readonly void Deconstruct(out float r, out float g, out float b)
        => (r, g, b) = (R, G, B);

    public readonly override string ToString()
        => $"({R:F2}, {G:F2}, {B:F2}, {A:F2})";

    public readonly string ToHexString()
        => $"{(int)R:X2}{(int)G:X2}{(int)B:X2}{(int)A:X2}";

    public readonly Color Inverted()
        => new(1.0f - R, 1.0f - G, 1.0f - B, A);

    public readonly Color Darkened(float amount)
        => new(R * (1.0f - amount), G * (1.0f - amount), B * (1.0f - amount), A);

    public readonly Color Lightened(float amount)
        => new(R + (1.0f - R) * amount, G + (1.0f - G) * amount, B + (1.0f - B) * amount, A);

    public static Color operator +(Color left, Color right)
        => new(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);

    public static Color operator -(Color left, Color right)
        => new(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);

    public static Color operator *(Color left, Color right)
        => new(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);

    public static Color operator *(Color color, float scalar)
        => new(color.R * scalar, color.G * scalar, color.B * scalar, color.A * scalar);

    public static Color operator /(Color color, float scalar)
        => new(color.R / scalar, color.G / scalar, color.B / scalar, color.A / scalar);

    public static Color FromRgba(float r, float g, float b, float a = 1.0f)
        => new(r, g, b, a);

    public static Color FromRgba(byte r, byte g, byte b, byte a = 255)
        => new(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);

    public static Color Lerp(Color from, Color to, float weight)
        => new(
            MathHelper.Lerp(from.R, to.R, weight),
            MathHelper.Lerp(from.G, to.G, weight),
            MathHelper.Lerp(from.B, to.B, weight),
            MathHelper.Lerp(from.A, to.A, weight)
            );

    [CLSCompliant(false)]
    public static Color FromRgba(uint packedValue)
    {
        byte r = (byte)((packedValue & 0xff_00_00_00) >> 0x18);
        byte g = (byte)((packedValue & 0x00_ff_00_00) >> 0x10);
        byte b = (byte)((packedValue & 0x00_00_ff_00) >> 0x08);
        byte a = (byte)((packedValue & 0x00_00_00_ff) >> 0x00);
        return FromRgba(r, g, b, a);
    }

    /// <summary>Construct a color from hsv.</summary>
    /// <param name="h">hue, 0.0f ~ 1.0f</param>
    /// <param name="s">saturation, 0.0f ~ 1.0f</param>
    /// <param name="v">value, 0.0f ~ 1.0f</param>
    /// <returns></returns>
    public static Color FromHsv(float hue, float saturation, float value, float alpha = 1.0f)
    {
        hue = (hue * 6.0f) % 6.0f;
        int i = (int)hue;

        float f = hue - i;
        float p = value * (1.0f - saturation);
        float q = value * (1.0f - saturation * f);
        float t = value * (1.0f - saturation * (1.0f - f));

        return i switch
        {
            0 => new(value, t, p, alpha),
            1 => new(q, value, p, alpha),
            2 => new(p, value, t, alpha),
            3 => new(p, q, value, alpha),
            4 => new(t, p, value, alpha),
            _ => new(value, p, q, alpha),
        };
    }
}