using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Salix;

[DebuggerDisplay("position: {Position}, color: {Color}, texCoord: {TextureCoord}")]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorTexture : IEquatable<VertexPositionColorTexture>
{
    public static readonly VertexDeclaration VertexDeclaration;

    public Vector3 Position;
    public Vector4 Color;
    public Vector2 TextureCoord;

    static VertexPositionColorTexture()
    {
        VertexDeclaration = new(VertexElementType.Vector3, VertexElementType.Color, VertexElementType.Vector2);
    }

    public VertexPositionColorTexture(Vector3 position, Vector4 color, Vector2 textureCoord)
    {
        Position = position;
        Color = color;
        TextureCoord = textureCoord;
    }

    public readonly override bool Equals(object? obj)
        => obj is VertexPositionColorTexture texture && Equals(texture);

    public readonly bool Equals(VertexPositionColorTexture other)
        => Position.Equals(other.Position) &&
           Color.Equals(other.Color) &&
           TextureCoord.Equals(other.TextureCoord);

    public readonly override int GetHashCode()
        => HashCode.Combine(Position, Color, TextureCoord);

    public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        => left.Equals(right);

    public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        => !(left == right);
}