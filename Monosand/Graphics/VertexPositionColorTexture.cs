using System.Numerics;
using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorTexture
{
    public static readonly VertexDeclaration VertexDeclatation;

    public Vector3 Position;
    public Vector4 Color;
    public Vector2 TextureCoord;

    static VertexPositionColorTexture()
    {
        VertexDeclatation = new(VertexElementType.Vector3, VertexElementType.Color, VertexElementType.Vector2);
    }

    public VertexPositionColorTexture(Vector3 position, Vector4 color, Vector2 textureCoord)
    {
        Position = position;
        Color = color;
        TextureCoord = textureCoord;
    }
}