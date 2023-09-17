using System.Drawing;

namespace Monosand;

public abstract class Platform
{
    /// <summary>Init this platform.</summary>
    internal abstract void Init();
    internal abstract WinImpl CreateWindowImpl(int width, int height, string title, Window window);
}