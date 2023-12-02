using System.Numerics;
using System.Runtime.Versioning;

using Monosand;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyGame : Game
{
    private Texture2D texture665x680;
    private Texture2D texture500x500;
    private Texture2D texture64x64;
    private Texture2D texture768x448;
    private SpriteFont sprFont;
    private SpriteBatch spriteBatch;
    private RenderTarget tempTarget;
    //private SpriteShader spriteEffect;

    Vector2 posBase;
    Vector2 position;
    Vector2 position2;

    float a;
    int times = 3;

    public MyGame()
    {
        texture665x680 = ResourceLoader.LoadTexture2D("TestAssets/665x680.png");
        texture500x500 = ResourceLoader.LoadTexture2D("TestAssets/500x500.png");
        texture64x64 = ResourceLoader.LoadTexture2D("TestAssets/64x64.png");
        texture768x448 = ResourceLoader.LoadTexture2D("TestAssets/768x448.png");

        try
        {
            sprFont = ResourceLoader.LoadSpriteFont("TestAssets/atlas.png", "TestAssets/atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the TestAssets folder of Test.Win32!");
        }
        tempTarget = new(RenderContext, 1200, 600);
        //spriteEffect = new SpriteShader(Game.ResourceLoader.LoadGlslShader("TestAssets/MyShader.vert", "TestAssets/MyShader.frag"));
        //spriteBatch = new SpriteBatch(Game, spriteEffect);
        spriteBatch = new(this);
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
        if (KeyboardState.IsJustPressed(Key.H))
            times += times / 3;
        if (KeyboardState.IsJustPressed(Key.J))
            times -= times / 3;
        posBase += dir * 400f * FrameTimeF;
        a += 2f * FrameTimeF;
        position = posBase + new Vector2(0f, (MathF.Sin(a / 20.0f) + 1f) / 2f * 300f);
        position2 = posBase + new Vector2(0f, (MathF.Sin(a / 14.0f) + 1f) / 2f * 300f);
        position = position.Floored();
        position2 = position2.Floored();
        Window.Title = $"Monosand Test.Win32 | {DateTime.Now} | FPS: {Fps:F2} | FrameTime: {FrameTime:F4} | Times: {times}";
    }

    public override void Render()
    {
        base.Render();
        RenderContext.Clear(Color.Known.CornflowerBlue);
        RenderContext.RenderTarget = tempTarget;
        {
            RenderContext.Clear(Color.Known.Black with { A = 0.2f });
            string str =
                $"DrawCalls: {LastDrawCalls}\n" +
                $"FrameTime: {FrameTime:F4}\n" +
                $"DrawTex repeats: {times}\n";

            for (int i = 0; i < times; i++)
                spriteBatch.DrawTexture(texture768x448, position + new Vector2(350f, 0f), scale: Vector2.One);
            spriteBatch.DrawText(sprFont, str, position2, Vector2.One);

            for (int i = 0; i < times; i++)
                spriteBatch.DrawCircle(
                texture665x680,
                Matrix3x2.CreateScale(75f, 75f) *
                Matrix3x2.CreateTranslation(500f, 250f) *
                Matrix3x2.CreateTranslation(position)
                );
        }
        RenderContext.RenderTarget = null;
        spriteBatch.DrawTexture(tempTarget.Texture, Vector2.One * 10.0f);
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