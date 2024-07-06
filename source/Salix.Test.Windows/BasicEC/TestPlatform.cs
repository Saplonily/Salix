using System.Numerics;
using Saladim.Salix;
using Saladim.Salix.EC;

namespace Test.Win32;

public class TestPlatform : Entity
{
    public readonly Vector2 Position;
    public readonly Vector2 Size;

    public TestPlatform(Vector2 position, Vector2 size)
    {
        this.Position = position;
        this.Size = size;
    }

    public override void Update()
    {
        base.Update();

    }

    public override void Render()
    {
        base.Render();
        var batch = MyGame.Current.SpriteBatch;
        batch.DrawTexture(batch.Texture1x1White, new DrawTransform(Position, Size));
    }
}