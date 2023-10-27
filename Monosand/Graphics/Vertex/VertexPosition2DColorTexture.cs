using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Monosand;

[DebuggerDisplay("position: {Position}, color: {Color}, texCoord: {TextureCoord}")]
[StructLayout(LayoutKind.Sequential)]
public struct VertexPosition2DColorTexture : IEquatable<VertexPosition2DColorTexture>
{
    public static readonly VertexDeclaration VertexDeclaration;

    public Vector2 Position;
    public Vector4 Color;
    public Vector2 TextureCoord;

    static VertexPosition2DColorTexture()
    {
        VertexDeclaration = new(VertexElementType.Vector2, VertexElementType.Color, VertexElementType.Vector2);
    }

    public VertexPosition2DColorTexture(Vector2 position, Vector4 color, Vector2 textureCoord)
    {
        Position = position;
        Color = color;
        TextureCoord = textureCoord;
    }

    public readonly override bool Equals(object? obj)
        => obj is VertexPosition2DColorTexture texture && Equals(texture);

    public readonly bool Equals(VertexPosition2DColorTexture other)
        => Position.Equals(other.Position) &&
           Color.Equals(other.Color) &&
           TextureCoord.Equals(other.TextureCoord);

    public readonly override int GetHashCode()
        => HashCode.Combine(Position, Color, TextureCoord);

    public static bool operator ==(VertexPosition2DColorTexture left, VertexPosition2DColorTexture right)
        => left.Equals(right);

    public static bool operator !=(VertexPosition2DColorTexture left, VertexPosition2DColorTexture right)
        => !(left == right);
}