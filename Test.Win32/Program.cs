using System.Numerics;
using System.Runtime.Versioning;

using Monosand;
using Monosand.Win32;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class MyMainWindow : Window
{
    private Texture2D texture665x680 = null!;
    private Texture2D texture500x500 = null!;
    private SpriteFont sprFont = null!;
    private SpriteBatch spriteBatch = null!;

    public override void OnCreated()
    {
        //texture665x680 = Game.ResourceLoader.LoadTexture2D("665x680.png");
        texture500x500 = Game.ResourceLoader.LoadTexture2D("500x500.png");
        try
        {
            sprFont = Game.ResourceLoader.LoadSpriteFont("atlas.png", "atlas_info.bin");
        }
        catch (FileNotFoundException)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas_info.bin' to the exe folder of the Test.Win32!.");
        }
        spriteBatch = new SpriteBatch(RenderContext);
    }

    Vector2 position;

    public override void Update()
    {
        base.Update();
        Vector2 dir = new();
        if (KeyboardState.IsPressing(Key.A))
            dir -= Vector2.UnitX;
        if (KeyboardState.IsPressing(Key.W))
            dir -= Vector2.UnitY;
        if (KeyboardState.IsPressing(Key.D))
            dir += Vector2.UnitX;
        if (KeyboardState.IsPressing(Key.S))
            dir += Vector2.UnitY;

        if (dir != Vector2.Zero)
            dir = Vector2.Normalize(dir);
        position += dir * 400f / 60f;
        a += 0.01f;
    }
    float a;
    public override void Render()
    {
        base.Render();

        //spriteBatch.DrawTexture(texture665x680, position, Vector2.One * 0.5f);//, Vector2.One * 0.5f);
        //spriteBatch.DrawTexture(textAtlas, position);
        //spriteBatch.DrawTexture(texture500x500, Vector2.Zero);
        string str = "这是一串...测试文本..换行！→\n然后...#[]{}'\",继续换行!\n我能吞下玻璃而不伤身体.\nThe quick brown fox jumps over the lazy dog.";
        spriteBatch.DrawText(sprFont, str, Vector2.One * 400f, Vector2.One / 2f, Vector2.One * 1.5f, a);
        //spriteBatch.DrawTexture(texture500x500, Vector2.Zero);
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