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

    private Vector2 pos;

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
        tex1.Filter = TextureFilterType.Nearest;
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
        pos += (PointerState.Position - pos) / 8f;

        spriteBatch.DrawCircle(tex1, new DrawTransform(pos), Color.Known.White, 32);
        spriteBatch.DrawCircle(tex1, new(pos, Vector2.Zero, new Vector2(100f / tex1.Width, 100f / tex1.Height), Ticks / 100f));
        string str = $"""
            LeftButton: {PointerState.IsLeftButtonPressing}
            MiddleButton: {PointerState.IsMiddleButtonPressing}
            RightButton: {PointerState.IsRightButtonPressing}
            Wheel: {PointerState.WheelOffset}
            """;
        spriteBatch.DrawText(sprFont, str, DrawTransform.None);
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