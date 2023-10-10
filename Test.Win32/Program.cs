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
            $"Game.Ticks: {Game.Ticks}\n" +
            $"Game.Ticks / 60d: {Game.Ticks / 60d:F2}\n" +
            $"Game.ExpectedFps: {Game.ExpectedFps}\n" +
            $"Game.ExpectedDelta: {Game.ExpectedDelta}";
        spriteBatch.DrawText(sprFont, str, new(MathF.Floor(position.X), MathF.Floor(position.Y)), Vector2.One);
        spriteBatch.Flush();
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