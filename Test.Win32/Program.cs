using System.Diagnostics;
using System.Numerics;

using Monosand;
using Monosand.Win32;

namespace Test.Win32;

public class MyMainWindow : Window
{
    VertexBuffer<VertexPositionColorTexture> vertexBuffer = null!;

    public VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[]
    {
        new(new(-0.8f, -0.8f, 0.0f), Vector4.One, new(0f, 0f)),
        new(new( 0.8f,  0.8f, 0.0f), Vector4.One, new(1f, 1f)),
        new(new( 0.8f, -0.8f, 0.0f), Vector4.One, new(1f, 0f)),

        new(new(-0.8f, -0.8f, 0.0f), Vector4.One, new(0f, 0f)),
        new(new(-0.8f,  0.8f, 0.0f), Vector4.One, new(0f, 1f)),
        new(new( 0.8f,  0.8f, 0.0f), Vector4.One, new(1f, 1f)),
    };

    float a = 0.0f;
    public unsafe override void OnCreated()
    {
        vertexBuffer = new(VertexPositionColorTexture.VertexDeclaration);
        vertexBuffer.SetData(Vertices);

        void* data = Interop.MsdLoadTextureFromFileRGBA("665x680.png", out int width, out int height);
        Texture2D tex = new(width, height, data);

        Interop.MsdFreeTexture(data);
    }

    public unsafe override void Render()
    {
        base.Render();
        a += 0.01f;
        if (a >= MathF.PI) a = -MathF.PI;

        DrawPrimitives(vertexBuffer, PrimitiveType.TriangleList);

        a += 0.5f;
        //DrawPrimitives(VertexPositionColorTexture.VertexDeclaration, PrimitiveType.TriangleList, Vertices);
        a -= 0.5f;
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new(new Win32Platform(), new MyMainWindow());
        game.Run();
    }
}
