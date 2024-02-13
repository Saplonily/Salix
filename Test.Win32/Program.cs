using System.Numerics;
using System.Runtime.Versioning;
using Monosand;
using Monosand.EC;
using static Test.Win32.MyGame;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class Program
{
    public static void Main()
    {
        try
        {
            MyGame game = new();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}