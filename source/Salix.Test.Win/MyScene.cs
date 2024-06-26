using System.Numerics;
using Salix;
using Salix.EC;

namespace Test.Win32;

public class MyScene : Scene
{
    public MyScene()
    {
        AddEntity(new TestPlatform(new(12f, 500f), new(1024f, 16f)));
        AddEntity(new TestPlatform(new(120f, 400f), new(512f, 16f)));
        AddEntity(new TestPlayer(new(300f, 0f)));
    }

    public override void Render()
    {
        base.Render();
        var batch = MyGame.Current.SpriteBatch;
        batch.DrawTexture(batch.Texture1x1White, new DrawTransform(Vector2.Zero, Vector2.One * 50f), Color.Known.Aqua with { A = 0.5f });
        batch.DrawCircle(Color.Known.Bisque, 100f, new DrawTransform(Vector2.One * 100f), 24);
    }
}
