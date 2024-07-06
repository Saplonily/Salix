using System.Numerics;

namespace Saladim.Salix.Test.Windows;

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
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        batch.DrawTexture(tex, new DrawTransform(new Vector2(100f + MathF.Sin(Ticks / 10f) * 100f, 100f), Vector2.Zero, new Vector2(4f, 4f), MathF.Sin(Ticks / 10f) * 0.2f));
        batch.DrawCircle(Color.Known.Black with { A = 0.2f }, 100f, new DrawTransform(Vector2.One * 200f, new Vector2(3f, 2f)), 48);
    }

    public override void Update()
    {
        if(Ticks % 10 == 0)
        {
            Window.Title = $"Salix.Test.Windows | Fps: {Fps} | FrameTime: {FrameTimeF:F2} | DrawCall: {LastDrawCalls}";
        }
    }
}
