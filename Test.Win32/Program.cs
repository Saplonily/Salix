using System.Diagnostics;
using System.Numerics;

using Monosand;
using Monosand.Win32;

namespace Test.Win32;

public class MyMainWindow : Window
{
    VertexBuffer<VertexPositionColorTexture> vertexBuffer = null!;
    VertexBuffer<VertexPositionColorTexture> vertexBuffer2 = null!;

    public ReadOnlySpan<VertexPositionColorTexture> Vertices => new VertexPositionColorTexture[]
    {
        new(new(-0.8f, -0.8f, 0.0f), new(Math.Abs(a), 0.0f, 0.0f, 1.0f), Vector2.Zero),
        new(new(0.8f, 0.8f, 0.0f), new(0.0f, 1.0f, 0.0f, 1.0f), Vector2.Zero),
        new(new(0.8f, -0.8f, 0.0f), new(0.0f, 0.0f, 1.0f, 1.0f), Vector2.Zero),

        new(new(-0.8f, -0.8f, 0.0f), new(1.0f, 0.0f, 0.0f, 1.0f), Vector2.Zero),
        new(new(-0.8f, 0.8f, 0.0f), new(0.0f, 1.0f, 0.0f, 1.0f), Vector2.Zero),
        new(new(0.8f, 0.8f, 0.0f), new(0.0f, Math.Abs(a), 1.0f, 1.0f), Vector2.Zero),
    };

    float a = 0.0f;
    public override void OnCreated()
    {
        a = 0.0f;
        vertexBuffer = new(VertexPositionColorTexture.VertexDeclaration);
        vertexBuffer2 = new(VertexPositionColorTexture.VertexDeclaration);
        vertexBuffer.SetData(Vertices);
        a += 1.0f;
    }

    public unsafe override void Render()
    {
        base.Render();
        a += 0.01f;
        if (a >= MathF.PI) a = -MathF.PI;
        DrawPrimitives(vertexBuffer, PrimitiveType.TriangleList);
        a += 0.5f;
        //vertexBuffer2.SetData(Vertices);
        //DrawPrimitives(vertexBuffer2, PrimitiveType.TriangleList);
        a -= 0.5f;
    }
}

public class Program
{
    public static void Main()
    {
        Stopwatch sw = new();
        sw.Start();

        Window win = new MyMainWindow();
        Platform pf = new Win32Platform();


        Game game = new(pf, win);
        game.Run();
    }
}
