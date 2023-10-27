using System.Diagnostics;
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
    [AllowNull] private RenderTarget tempTarget;

    Vector2 posBase;
    Vector2 position;
    Vector2 position2;

    Stopwatch sw = new();

    float a;
    int times = 3;

    public MyMainWindow(Game game) : base(game)
    {
    }

    public override void OnCreated()
    {
        texture665x680 = Game.ResourceLoader.LoadTexture2D("665x680.png");
        texture500x500 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        try
        {
            sprFont = Game.ResourceLoader.LoadSpriteFont("atlas.png", "atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the exe folder of Test.Win32!");
        }
        spriteBatch = new SpriteBatch(Game);
        tempTarget = new(Game.RenderContext, 300, 600);
    }

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
        position2 = posBase + new Vector2(0f, (MathF.Sin(a / 1.4f) + 1f) / 2f * 300f);
        Title = $"Monosand Test.Win32 {DateTime.Now}";
    }

    public override void Render()
    {
        base.Render();

        if (KeyboardState.IsJustPressed(Key.H))
            times += times / 3;
        if (KeyboardState.IsJustPressed(Key.J))
            times -= times / 3;

        RectangleProp<Color> c = new(Color.Known.AliceBlue, Color.Known.Yellow, Color.Known.RosyBrown, Color.Known.Black);

        Game.RenderContext.RenderTarget = tempTarget;

        Game.RenderContext.Clear(Color.Known.Transparent with { A = 20 });
        spriteBatch.DrawTexture(texture665x680, position, Vector2.One / 2f, c);
        string str =
            $"DrawCalls: {Game.LastDrawCalls}\n" +
            $"Ticks: {Game.Ticks}\n" +
            $"ExpectedFps: {Game.ExpectedFps:F4}\n" +
            $"Fps: {Game.Fps:F4}\n" +
            $"DrawText repeats: {times}\n" +
            $"IsRunningSlowly: {Game.IsRunningSlowly}";
        for (int i = 0; i < times; i++)
            spriteBatch.DrawText(sprFont, str, position, Vector2.One);
        spriteBatch.Flush();

        Game.RenderContext.RenderTarget = null;
        spriteBatch.DrawTexture(tempTarget.Texture, Vector2.One * 10f);
        spriteBatch.Flush();

        spriteBatch.DrawTexture(texture665x680, position2 + new Vector2(350f, 0f), Vector2.One / 3f, c);
        spriteBatch.Flush();
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new(new Win32Platform());
        var win = new MyMainWindow(game);
        game.Window = win;
        game.ExpectedFps = game.VSyncFps;
        //game.VSyncEnabled = true;
        game.Run();
    }
}