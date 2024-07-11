using System.Numerics;

namespace Saladim.Salix.Tests.BasicTest;

public class MyGame : Game
{
    private FileSystemResourceManager res;
    private SpriteBatch batch;
    private SpriteShader myShader;
    private Texture2D tex;

    public MyGame()
    {
        res = new(ResourceLoader);

        batch = new(this);

        myShader = new(res.Load<Shader>("MyShader"));
        batch.SpriteShader = myShader;

        tex = res.Load<Texture2D>("64x64");
        tex.Filter = TextureFilterType.Nearest;

        Sampler linearSampler = new(RenderContext, TextureFilterType.Linear, TextureWrapType.Repeat);
        RenderContext.SetSampler(0, linearSampler);
        linearSampler.Dispose();
        linearSampler = new(RenderContext, TextureFilterType.Linear, TextureWrapType.Repeat);
        RenderContext.SetSampler(0, linearSampler);
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        batch.DrawTexture(tex, new DrawTransform(new Vector2(100f + MathF.Sin(Ticks / 10f) * 100f, 100f), Vector2.Zero, Vector2.One * 10f));
    }

    public override void Update()
    {
        if (Ticks % 10 == 0)
            Window.Title = $"Salix.Test.Windows | Fps: {Fps} | FrameTime: {FrameTimeF:F2} | DrawCall: {LastDrawCalls}";

    }
}
