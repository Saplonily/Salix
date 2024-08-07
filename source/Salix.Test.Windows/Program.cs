﻿using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

namespace Saladim.Salix.Tests;

public class Program
{
    public static void Main()
    {
#if DEBUG
        BasicEC.MyGame game = new();
        game.Run();
#else
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