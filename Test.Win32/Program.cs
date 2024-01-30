using System.Numerics;
using System.Runtime.Versioning;
using Monosand;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyGame : Game
{
    private Texture2D tex1;
    private SpriteFont sprFont;
    private SpriteBatch spriteBatch;

    public MyGame()
    {
        try
        {
            sprFont = ResourceLoader.LoadSpriteFont("TestAssets/atlas.png", "TestAssets/atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the TestAssets folder of Test.Win32!");
        }
        tex1 = ResourceLoader.LoadTexture2D("TestAssets/768x448.png");
        spriteBatch = new(this);
    }

    public override void Update()
    {
        base.Update();
        if (Ticks % 10 == 0)
            Window.Title = $"Monosand Test.Win32 | {DateTime.Now} | FPS: {Fps:F2} | FrameTime: {FrameTime:F4}";
    }

    public override void Render()
    {
        base.Render();
        RenderContext.Clear(Color.Known.CornflowerBlue);
        Vector2 pos = PointerState.Position;
        Matrix3x2 mat = Matrix3x2.Identity * Matrix3x2.CreateScale(100f) * Matrix3x2.CreateTranslation(350f, 100f);
        spriteBatch.DrawCircle(spriteBatch.Texture1x1White, mat, Color.Known.Black, 32);

        mat = Matrix3x2.Identity * Matrix3x2.CreateScale(100f) * Matrix3x2.CreateTranslation(130f, 100f);
        spriteBatch.DrawCircle(tex1, mat, Color.Known.White, 32);
        spriteBatch.Flush();
    }
}

public class Program
{
    public static void Main()
    {
        MyGame game = new();
        game.ExpectedFps = game.VSyncFps;
        //game.VSyncEnabled = true;
        game.Run();
    }
}