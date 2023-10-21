using System.Drawing;

namespace Monosand;

internal unsafe abstract class WindowImpl
{
    internal abstract string Title { get; set; }
    internal abstract Point Position { get; set; }
    internal abstract Size Size { get; set; }

    internal abstract void Destroy();

    internal abstract void PollEvents();

    internal abstract void Show();

    internal abstract void Hide();

    internal abstract void SwapBuffers();
}