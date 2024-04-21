using System.Drawing;
using System.Numerics;
using Monosand;
using Monosand.EC;
using Color = Monosand.Color;

namespace Test.Win32;

public class TestPlayer : Entity
{
    private static Vector2 size = new(50f, 50f);
    private Vector2 position;
    private Vector2 velocity;

    public TestPlayer(Vector2 position)
    {
        this.position = position;
    }

    public override void Update()
    {
        base.Update();
        var game = MyGame.Current;
        var ks = MyGame.Current.KeyboardState;
        velocity += Vector2.UnitY * 900f * game.ExpectedFrameTimeF;

        if (ks.IsPressing(Key.A))
        {
            TryMove(-Vector2.UnitX * 640f * game.ExpectedFrameTimeF);
        }
        if (ks.IsPressing(Key.D))
        {
            TryMove(Vector2.UnitX * 640f * game.ExpectedFrameTimeF);
        }
        if (ks.IsJustPressed(Key.W))
        {
            velocity.Y = -400f;
        }

        TestPlatform? p = TryMove(velocity * game.ExpectedFrameTimeF);
        if (p is not null)
        {
            position.Y = p.Position.Y - size.Y;
            velocity.Y = 0f;
        }
    }

    private TestPlatform? TryMove(Vector2 move)
    {
        position += move;
        var thisRect = new RectangleF((PointF)position, (SizeF)size);
        foreach (var p in Scene.Entities.OfType<TestPlatform>())
        {
            var pRect = new RectangleF((PointF)p.Position, (SizeF)p.Size);
            if (thisRect.IntersectsWith(pRect))
            {
                position -= move;
                return p;
            }
        }
        return null;
    }

    public override void Render()
    {
        base.Render();
        var batch = MyGame.Current.SpriteBatch;
        batch.DrawTexture(batch.Texture1x1White, new DrawTransform(position, size), Color.Known.BlueViolet, Vector2.Zero, Vector2.One);
    }
}
