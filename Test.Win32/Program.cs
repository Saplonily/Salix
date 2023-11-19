﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Versioning;

using Monosand;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    private Texture2D texture665x680 = null!;
    private Texture2D texture500x500 = null!;
    private Texture2D texture64x64 = null!;
    private Texture2D texture768x448 = null!;
    private SpriteFont sprFont = null!;
    private SpriteBatch spriteBatch = null!;
    private RenderTarget tempTarget = null!;
    private SpriteShader spriteEffect = null!;
    private AudioData audioData = null!;

    Vector2 posBase;
    Vector2 position;
    Vector2 position2;

    float a;
    int times = 3;

    public MyMainWindow(Game game) : base(game)
    {
    }

    public override void OnInitialize()
    {
        texture665x680 = Game.ResourceLoader.LoadTexture2D("TestAssets/665x680.png");
        texture500x500 = Game.ResourceLoader.LoadTexture2D("TestAssets/500x500.png");
        texture64x64 = Game.ResourceLoader.LoadTexture2D("TestAssets/64x64.png");
        texture768x448 = Game.ResourceLoader.LoadTexture2D("TestAssets/768x448.png");
        //audioData = Game.ResourceLoader.LoadAudio("TestAssets/test_rock.wav");

        try
        {
            sprFont = Game.ResourceLoader.LoadSpriteFont("TestAssets/atlas.png", "TestAssets/atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the TestAssets folder of Test.Win32!");
        }
        tempTarget = new(Game.RenderContext, 1200, 600);
        spriteEffect = new SpriteShader(Game.ResourceLoader.LoadGlslShader("TestAssets/MyShader.vert", "TestAssets/MyShader.frag"));
        //spriteBatch = new SpriteBatch(Game, spriteEffect);
        spriteBatch = new(Game);
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
        posBase += dir * 400f * Game.FrameTimeF;
        a += 2f * Game.FrameTimeF;
        position = posBase + new Vector2(0f, (MathF.Sin(a / 20.0f) + 1f) / 2f * 300f);
        position2 = posBase + new Vector2(0f, (MathF.Sin(a / 14.0f) + 1f) / 2f * 300f);
        position = position.Floored();
        position2 = position2.Floored();
        Title = $"Monosand Test.Win32 | {DateTime.Now} | FPS: {Game.Fps:F2} | FrameTime: {Game.FrameTime:F4} | Times: {times}";
    }

    public override void Render()
    {
        base.Render();
        Game.RenderContext.Clear(Color.Known.CornflowerBlue);
        Game.RenderContext.RenderTarget = tempTarget;
        {
            Game.RenderContext.Clear(Color.Known.Black with { A = 0.2f });
            string str =
                $"DrawCalls: {Game.LastDrawCalls}\n" +
                $"FrameTime: {Game.FrameTime:F4}\n" +
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
        Game.RenderContext.RenderTarget = null;
        spriteBatch.DrawTexture(tempTarget.Texture, Vector2.One * 10.0f);
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new();
        game.Window = new MyMainWindow(game);
        game.ExpectedFps = game.VSyncFps;
        //game.VSyncEnabled = true;
        game.Run();
    }
}