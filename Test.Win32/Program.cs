using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    private Texture2D texture665x680 = null!;
    private Texture2D texture500x500 = null!;
    private SpriteFont sprFont = null!;
    private SpriteBatch spriteBatch = null!;

    public override void OnCreated()
    {
        //texture665x680 = Game.ResourceLoader.LoadTexture2D("665x680.png");
        texture500x500 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        try
        {
            sprFont = Game.ResourceLoader.LoadSpriteFont("atlas.png", "atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the exe folder of the Test.Win32!.");
        }
        spriteBatch = new SpriteBatch(RenderContext);
    }

    Vector2 position;

    public override void Update()
    {
        base.Update();
        Vector2 dir = new();
        if (KeyboardState.IsPressing(Key.A))
            dir -= Vector2.UnitX;
        if (KeyboardState.IsPressing(Key.W))
            dir -= Vector2.UnitY;
        if (KeyboardState.IsPressing(Key.D))
            dir += Vector2.UnitX;
        if (KeyboardState.IsPressing(Key.S))
            dir += Vector2.UnitY;

        if (dir != Vector2.Zero)
            dir = Vector2.Normalize(dir);
        position += dir * 400f / 60f;
    }

    public override void Render()
    {
        base.Render();

        string str =
            $"Ticks: {Game.Ticks}\n" +
            $"Ticks / {Game.ExpectedFps}: {Game.Ticks / Game.ExpectedFps:F4}\n" +
            $"ExpectedFps: {Game.ExpectedFps:F4}\n" +
            $"ExpectedDelta: {Game.ExpectedDelta:F4}\n" +
            $"Fps: {Game.Fps:F4}\n" +
            $"Delta: {Game.Delta:F4}";
        for (int i = 0; i < 10; i++)
            spriteBatch.DrawText(sprFont, str, new(0, 0), Vector2.One);
        spriteBatch.Flush();
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new(new Win32Platform(), new MyMainWindow());
        game.ExpectedFps = 2d;
        game.VSyncEnabled = true;
        game.Run();
    }
}