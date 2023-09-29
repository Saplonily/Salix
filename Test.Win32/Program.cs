using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;
[assembly: SupportedOSPlatform("windows")]

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
    Texture2D tex = null!;
    Texture2D tex2 = null!;
    public unsafe override void OnCreated()
    {
        vertexBuffer = new(VertexPositionColorTexture.VertexDeclaration);
        vertexBuffer.SetData(Vertices);

        tex = Game.ResourceLoader.LoadTexture2D("665x680.png");
        tex2 = Game.ResourceLoader.LoadTexture2D("500x500.png");
    }

    public unsafe override void Render()
    {
        base.Render();
        a += 0.1f;
        if (a >= MathF.PI)
            a = -MathF.PI;

        RenderContext.SetTexture(0, a > 0 ? tex : tex2);
        RenderContext.DrawPrimitives(vertexBuffer, PrimitiveType.TriangleList);
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
