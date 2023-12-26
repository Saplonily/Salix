using System.Numerics;
using System.Runtime.Versioning;
using Monosand;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyGame : Game
{
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

        spriteBatch.DrawText(sprFont, "Here is the text.", Vector2.Zero);
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