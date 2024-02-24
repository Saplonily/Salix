using System.Drawing;
using Monosand;
using Monosand.EC;
using Color = Monosand.Color;

namespace Test.Win32;

public class MyGame : ECGame
{
    public SpriteFont SpriteFont;
    public FileSystemResourceManager ResourceManager { get; private set; }

    public static MyGame Instance { get; private set; } = null!;

    public MyGame()
    {
        Instance = this;
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

        Scene scene = new();
        scene.AddEntity(new TestEntity());
        scene.AddEntity(new TestEntity());
        GotoScene(scene);
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
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        base.Render();

        string str = $"""
            LeftButton: {PointerState.IsLeftButtonPressing}
            MiddleButton: {PointerState.IsMiddleButtonPressing}
            RightButton: {PointerState.IsRightButtonPressing}
            Wheel: {PointerState.WheelOffset}
            """;

        SpriteBatch.DrawText(SpriteFont, str, DrawTransform.None);

        SpriteBatch.Flush();
    }
}