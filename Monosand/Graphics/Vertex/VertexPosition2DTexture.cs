using System.Numerics;
using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPosition2DTexture : IEquatable<VertexPosition2DTexture>
{
    public static readonly VertexDeclaration VertexDeclaration;

    public Vector2 Position;
    public Vector2 TextureCoord;

    static VertexPosition2DTexture()
    {
        VertexDeclaration = new(VertexElementType.Vector2, VertexElementType.Vector2);
    }

    public VertexPosition2DTexture(Vector2 position, Vector2 textureCoord)
    {
        Position = position;
        TextureCoord = textureCoord;
    }

    public readonly override bool Equals(object? obj)
        => obj is VertexPosition2DTexture texture && Equals(texture);


    public readonly bool Equals(VertexPosition2DTexture other)
        => Position.Equals(other.Position) &&
           TextureCoord.Equals(other.TextureCoord);

    public readonly override int GetHashCode()
        => HashCode.Combine(Position, TextureCoord);

    public static bool operator ==(VertexPosition2DTexture left, VertexPosition2DTexture right)
        => left.Equals(right);

    public static bool operator !=(VertexPosition2DTexture left, VertexPosition2DTexture right)
        => !(left == right);
}