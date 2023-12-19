using System.Numerics;
using System.Runtime.Versioning;

using Monosand;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyGame : Game
{
    private SpriteFont sprFont;
    private SpriteBatch spriteBatch;
    private Sound soundRock = null!;
    private Sound soundPiano = null!;

    public MyGame()
    {
        soundRock = new(ResourceLoader.LoadAudio("TestAssets/test_rock.wav"));
        soundPiano = new(ResourceLoader.LoadAudio("TestAssets/test_piano32000.wav"));

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
    SoundInstance? si;
    public override void Update()
    {
        base.Update();
        if (Ticks % 10 == 0)
            Window.Title = $"Monosand Test.Win32 | {DateTime.Now} | FPS: {Fps:F2} | FrameTime: {FrameTime:F4}";

        if (KeyboardState.IsJustPressed(Key.A))
        {
            si = soundPiano.CreateInstance();
            //si.PlaySpeed = 2.0f;
            //si.Next.PlaySpeed = 2.0f;
            AudioContext.Play(si);
        }
        if (KeyboardState.IsJustPressed(Key.E))
        {
            si = soundPiano.Play();
        }
        if (KeyboardState.IsJustPressed(Key.G))
        {
            GC.Collect();
        }
        if (KeyboardState.IsJustPressed(Key.N))
        {
            si = null;
        }
    }

    public override void Render()
    {
        base.Render();
        RenderContext.Clear(Color.Known.CornflowerBlue);
        string text = """
            Press <A> to play the audio.
            Press <S> to play another audio.
            Press <P> to pause the audio or resume.
            Press <J> to stop the audio.
            """;
        if (si is not null)
        {
            text += $"\n{si.PlayedDuration} / {si.Sound.AudioData.Duration}";
        }

        spriteBatch.DrawText(sprFont, text, Vector2.One * 100.0f, Vector2.One * 1f);
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