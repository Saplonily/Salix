using System.Numerics;
using Monosand;
using Monosand.EC;
using Color = Monosand.Color;

namespace Test.Win32;

public class MyGame : ECGame
{
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
        ExpectedFps = VSyncFps;
        //VSyncEnabled = true;

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
            Window.Title = $"Monosand | Fps: {Fps} | DrawCall: {LastDrawCalls} | Entities: {Scene.Entities.Count}";
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        base.Render();
    }
}