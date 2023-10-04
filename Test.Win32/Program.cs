using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    private Texture2D tex = null!;
    private Texture2D tex2 = null!;
    private SpriteBatch spriteBatch = null!;

    public override void OnCreated()
    {
        tex = Game.ResourceLoader.LoadTexture2D("665x680.png");
        tex2 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        spriteBatch = new SpriteBatch(RenderContext);
    }

    float a = 0;
    public override void Render()
    {
        base.Render();
        a += 0.1f;
        float r = MathF.Sin(a / 5f) * 5f;
        spriteBatch.DrawTexture(tex, new(tex.Width / 2, tex.Height / 2), Vector2.One * 0.5f, Vector2.One * 0.5f, r);
        spriteBatch.Flush();
    }
}

public class Program
{
    public static void Main()
    {
        Game game = new(new Win32Platform(), new MyMainWindow());
        game.Run();
    }
}

public class SomeClass : ISomeInterface
{
    static void ISomeInterface.TestMethod()
    {
        Console.WriteLine("Hi in SomeClass!");
    }
}

public interface ISomeInterface
{
    public virtual static void TestMethod()
    {
        Console.WriteLine("Hi!");
    }
}