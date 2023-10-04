using System;
using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    float a;
    Texture2D tex = null!;
    Texture2D tex2 = null!;
    Shader shader = null!;
    SpriteBatch batch = null!;
    Matrix4x4 mat;

    public unsafe override void OnCreated()
    {
        tex = Game.ResourceLoader.LoadTexture2D("665x680.png");
        tex2 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        shader = Game.ResourceLoader.LoadGlslShader("test.vert", "test.frag");
        batch = new(RenderContext);
    }

    public override void OnResized(int width, int height)
    {
        base.OnResized(width, height);
        mat = Matrix4x4.Identity;
        mat *= Matrix4x4.CreateTranslation(-width / 2f, -height / 2f, 0f);
        mat *= Matrix4x4.CreateScale(2f / width, -2f / height, 1f);
    }

    public override void Update()
    {
        base.Update();
        if (KeyboardState.IsJustPressed(Key.D))
        {
            Console.WriteLine("D just pressed.");
        }
    }

    public unsafe override void Render()
    {
        base.Render();
        a += 0.1f;
        if (a >= Math.PI)
            a = -(float)Math.PI;
        shader.Use();
        shader.GetParameter("tex0"u8).Set(0);
        shader.GetParameter("projectionMat"u8).Set(ref mat);

        for (int i = 0; i < 20000; i++)
            batch.DrawTexture((i % 2) == 0 ? tex : tex, new Vector2((i * 4) % 1200, (i * 20) % 600), Vector2.One * 0.1f, Color.White);
        batch.Flush();
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
