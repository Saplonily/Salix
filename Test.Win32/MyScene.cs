using System.Numerics;
using Monosand.EC;

namespace Test.Win32;

public class MyScene : Scene
{
    public MyScene()
    {
        AddEntity(new TestPlatform(new(12f, 500f), new(1024f, 16f)));
        AddEntity(new TestPlatform(new(120f, 400f), new(512f, 16f)));
        AddEntity(new TestPlayer(new(300f, 0f)));
    }
}
