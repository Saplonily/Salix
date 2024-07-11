using System.Numerics;

namespace Saladim.Salix.Tests.SyncTest;

public class MyGame : Game
{
    private SpriteBatch batch;
    private float v;
    private int t = 0;

    public MyGame()
    {
        batch = new(this);
        TargetFps = 75f;
        //VSyncEnabled = true;
    }

    public override void Update()
    {
        base.Update();
        v = MathF.Sin(Ticks / 50f) * 300f;

        if (Ticks % 10 == 0)
            Window.Title = $"Salix.Test.Windows | SyncTest | Fps: {Fps:F4}/{TargetFps:F4} | FT: {FrameTime:F4}/{TargetFrameTime:F4} | Times: {t * 100}";
        if (KeyboardState.IsPressing(Key.D))
            t++;
    }

    public override void Render()
    {
        base.Render();
        RenderContext.Clear(Color.Known.Black);
        for (int i = 0; i < 100 * t; i++)
            batch.DrawTexture(batch.Texture1x1White, new DrawTransform(new Vector2(350f + v, 20f), Vector2.Zero, Vector2.One * 100f));
    }
}