using System.Diagnostics;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

namespace Test.Win32;

public class Program
{
    public static void Main()
    {
        MyGame game = new();
        game.Run();
#if !DEBUG
        try
        {
            MyGame game = new();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            using (StreamWriter sw = new("error.txt", false))
            {
                sw.WriteLine("Test.Win32 Game Error");
                for (int i = 0; i < 64; i++)
                    sw.Write("=");
                sw.WriteLine();
                sw.WriteLine(ex.ToString());
            }
            Process.Start("notepad.exe", "error.txt");
        }
#endif
    }
}