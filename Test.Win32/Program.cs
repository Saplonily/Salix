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
        spriteBatch.DrawTexture(tex1, pos);
        spriteBatch.DrawText(sprFont, "Here is some texts.\nWith NewLines\nMore NewLines.", pos, Vector2.Zero, -0.1f);
        spriteBatch.DrawText(sprFont, "Here is some texts.\nWith NewLines.", pos * 1.5f, Vector2.Zero, -0.1f);
        var texCoord = new TriangleProperty<Vector2>(Vector2.Zero, new(0.4f, 0.0f), new(0.2f, 1f));
        var trianglePos = new TriangleProperty<Vector2>(Vector2.Zero, new Vector2(0.4f, 0.0f) * 400f, new Vector2(0.2f, 1f) * 400f);
        spriteBatch.DrawTriangle(tex1, trianglePos, Matrix3x2.CreateTranslation(100f, 100f), texCoord, Color.Known.Red);
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