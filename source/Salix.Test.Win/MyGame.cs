using System.Buffers;
using System.Numerics;
using Salix;
using Salix.EC;
using Color = Salix.Color;

namespace Test.Win32;

public class MyGame : ECGame
{
    public Texture2D TestTexture;
    public SpriteFont SpriteFont;

    public FileSystemResourceManager ResourceManager { get; private set; }

    public new static MyGame Current => (MyGame)ECGame.Current;

    public MyGame()
    {
        ResourceManager = new(ResourceLoader);
        try
        {
            SpriteFont = ResourceManager.Load<SpriteFont>("atlas");
        }
        catch (FileNotFoundException e)
        {
            throw new Exception("Run TextAtlasMaker and copy the 'atlas.png' and 'atlas.bin' to the Content folder of Test.Win32!", e);
        }
        TestTexture = ResourceManager.Load<Texture2D>("64x64");
        ExpectedFps = VSyncFps;
        VSyncEnabled = true;
        GotoScene(new MyScene());
    }

    public override void Update()
    {
        base.Update();
        if (KeyboardState.IsJustPressed(Key.Esc))
            RequestExit();
        if (KeyboardState.IsPressing(Key.S))
        {
            Window.Width -= 2;
            Window.X += 1;
        }
        if (Ticks % 10 == 0)
            Window.Title = $"Saladim.Salix | Fps: {Fps} | DrawCall: {LastDrawCalls} | Entities: {Scene.Entities.Count}";
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        base.Render();
    }
}