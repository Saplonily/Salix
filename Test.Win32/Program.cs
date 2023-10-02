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
        new(new(000f, 000f, 0f), Vector4.One, new(0f, 0f)),
        new(new(300f, 300f, 0f), Vector4.One, new(1f, 1f)),
        new(new(300f, 000f, 0f), Vector4.One, new(1f, 0f)),

        new(new(000f, 000f, 0f), Vector4.One, new(0f, 0f)),
        new(new(000f, 300f, 0f), Vector4.One, new(0f, 1f)),
        new(new(300f, 300f, 0f), Vector4.One, new(1f, 1f)),
    };

    float a = 0.0f;
    Texture2D tex = null!;
    Texture2D tex2 = null!;
    Shader ourShader = null!;
    Matrix4x4 mat;
    public unsafe override void OnCreated()
    {
        vertexBuffer = new(VertexPositionColorTexture.VertexDeclaration);
        vertexBuffer.SetData(Vertices);

        tex = Game.ResourceLoader.LoadTexture2D("665x680.png");
        tex2 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        ourShader = Game.ResourceLoader.LoadGlslShader("test.vert", "test.frag");
    }

    public override void OnResized(int width, int height)
    {
        base.OnResized(width, height);
        mat = Matrix4x4.Identity;
        mat *= Matrix4x4.CreateTranslation(-width / 2f, -height / 2f, 0f);
        mat *= Matrix4x4.CreateScale(2f / width, -2f / height, 1f);
    }

    public unsafe override void Render()
    {
        base.Render();
        a += 0.1f;
        if (a >= Math.PI)
            a = -(float)Math.PI;
        ourShader.Use();
        ourShader.GetParameter("tex0"u8).Set(0);
        ourShader.GetParameter("projectionMat"u8).Set(ref mat);
        RenderContext.SetTexture(1, tex);

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
