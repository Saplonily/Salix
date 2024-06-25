using System.Numerics;
using System.Runtime.InteropServices;

namespace Salix;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionTexture : IEquatable<VertexPositionTexture>
{
    public static readonly VertexDeclaration VertexDeclaration;

    public Vector3 Position;
    public Vector2 TextureCoord;

    static VertexPositionTexture()
    {
        VertexDeclaration = new(VertexElementType.Vector3, VertexElementType.Vector2);
    }

    public VertexPositionTexture(Vector3 position, Vector2 textureCoord)
    {
        Position = position;
        TextureCoord = textureCoord;
    }

    public readonly override bool Equals(object? obj)
        => obj is VertexPositionTexture texture && Equals(texture);


    public readonly bool Equals(VertexPositionTexture other)
        => Position.Equals(other.Position) &&
           TextureCoord.Equals(other.TextureCoord);

    public readonly override int GetHashCode()
        => HashCode.Combine(Position, TextureCoord);

    public static bool operator ==(VertexPositionTexture left, VertexPositionTexture right)
        => left.Equals(right);

    public static bool operator !=(VertexPositionTexture left, VertexPositionTexture right)
        => !(left == right);
}