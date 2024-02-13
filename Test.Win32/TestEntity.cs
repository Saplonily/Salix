using System.Numerics;
using Monosand;
using Monosand.EC;

namespace Test.Win32;

public class TestEntity : Entity
{
    private readonly Texture2D tex;
    private Vector2 pos;
    private Vector2 offset;

    public TestEntity()
    {
        tex = null!;
        for (int i = 0; i < 1000; i++)
            tex = MyGame.Instance.ResourceManager.Load<Texture2D>("768x448");
        offset = new((float)(Random.Shared.NextDouble() * 100d));
    }

    public override void Update()
    {
        base.Update();
        pos += (MyGame.Instance.PointerState.Position - pos) / 24f;
        if (MyGame.Instance.KeyboardState.IsJustPressed(Key.A))
        {
            Scene.RemoveEntity(this);
        }
    }

    public override void Render()
    {
        base.Render();
        var game = MyGame.Instance;

        game.SpriteBatch.DrawCircle(tex, new(pos + offset, Vector2.Zero, new Vector2(200f / tex.Width, 200f / tex.Height), game.Ticks / 100f), 32);
    }
}