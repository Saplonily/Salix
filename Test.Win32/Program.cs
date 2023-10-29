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
    [AllowNull] private Texture2D texture64x64;
    [AllowNull] private SpriteFont sprFont;
    [AllowNull] private SpriteBatch spriteBatch;
    [AllowNull] private RenderTarget tempTarget;
    private TextureFilterType filterType;
    private TextureWrapType wrapType;

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
        texture64x64 = Game.ResourceLoader.LoadTexture2D("64x64.png");
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
        position.Floor();
        position2.Floor();
        Title = $"Monosand Test.Win32 {DateTime.Now}";

        if (KeyboardState.IsJustPressed(Key.F))
        {
            filterType += 1;
            if (filterType == TextureFilterType.LinearMipmapLinear)
                filterType = TextureFilterType.Linear;
            (texture500x500.Filter, texture64x64.Filter, texture665x680.Filter) = (filterType, filterType, filterType);
        }
        if (KeyboardState.IsJustPressed(Key.P))
        {
            wrapType += 1;
            if (wrapType == TextureWrapType.MirroredRepeat + 1)
                wrapType = TextureWrapType.ClampToEdge;
            (texture500x500.Wrap, texture64x64.Wrap, texture665x680.Wrap) = (wrapType, wrapType, wrapType);
        }
    }

    public override void Render()
    {
        base.Render();
        Game.RenderContext.Clear(Color.Known.CornflowerBlue);

        if (KeyboardState.IsJustPressed(Key.H))
            times += times / 3;
        if (KeyboardState.IsJustPressed(Key.J))
            times -= times / 3;

        string str =
            $"DrawCalls: {Game.LastDrawCalls}\n" +
            $"FrameTime: {Game.FrameTime:F4}\n" +
            $"DrawTex repeats: {times}\n" +
            $"TextureFilter: {filterType}, F to change\n" +
            $"TextureWrap: {wrapType}, P to change";

        for (int i = 0; i < times; i++)
            spriteBatch.DrawTexture(
                texture64x64,
                position2 + new Vector2(350f, 0f),
                origin: Vector2.Zero, scale: Vector2.One * 3f,
                radians: 0.1f + i / 50.0f, color: Color.Known.White,
                textureTopLeft: Vector2.Zero, textureBottomRight: Vector2.One * 1.2f
                );
        spriteBatch.DrawText(sprFont, str, position, Vector2.One);
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