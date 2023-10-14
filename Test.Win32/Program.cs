using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    [AllowNull] private Texture2D texture665x680;
    [AllowNull] private Texture2D texture500x500;
    [AllowNull] private SpriteFont sprFont;
    [AllowNull] private SpriteBatch spriteBatch;

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

    Vector2 posBase;
    Vector2 position;

    float a;
    int times = 3;

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
        posBase += dir * 400f * Game.FrameTimeF;
        a += 2f * Game.FrameTimeF;
        position = posBase + new Vector2(0f, (MathF.Sin(a) + 1f) / 2f * 300f);
    }


    public override void Render()
    {
        base.Render();

        if (KeyboardState.IsJustPressed(Key.H))
            times += times / 3;
        if (KeyboardState.IsJustPressed(Key.J))
            times -= times / 3;

        spriteBatch.DrawTexture(texture500x500, position, Vector2.One / 2f);
        string str =
            $"Ticks: {Game.Ticks}\n" +
            $"Ticks / {Game.Fps:F2}: {Game.Ticks / Game.Fps:F4}\n" +
            $"ExpectedFps: {Game.ExpectedFps:F4}\n" +
            $"Fps: {Game.Fps:F4}\n" +
            $"FrameTime: {Game.FrameTime:F4}\n" +
            $"VSyncFps: {Game.VSyncFps}\n" +
            $"VSyncEnabled: {Game.VSyncEnabled}\n" +
            $"DrawText repeats: {times}\n" +
            $"IsRunningSlowly: {Game.IsRunningSlowly}";
        

        for (int i = 0; i < times; i++)
            spriteBatch.DrawText(sprFont, str, position, Vector2.One);
        spriteBatch.Flush();
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new(new Win32Platform(), new MyMainWindow());
        game.ExpectedFps = game.VSyncFps;
        //game.VSyncEnabled = true;
        game.Run();
    }
}