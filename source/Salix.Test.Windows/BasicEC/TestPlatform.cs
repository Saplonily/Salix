using System.Numerics;
using Saladim.Salix.EC;

namespace Saladim.Salix.Tests.BasicEC;

public class TestPlatform : Entity
{
    public readonly Vector2 Position;
    public readonly Vector2 Size;

    public TestPlatform(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
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