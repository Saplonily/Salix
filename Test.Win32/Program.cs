using System.Diagnostics;

using Monosand;
using Monosand.Win32;

namespace Test.Win32;

public class MyMainWindow : Window
{

}

public class Program
{
    public static void Main(string[] args)
    {
        Stopwatch sw = new();
        sw.Start();

        Window win = new MyMainWindow();
        Platform pf = new Win32Platform();


        Game game = new(pf, win);
        game.Run();
    }
}
