using System.Drawing;

namespace Saladim.Salix;

internal interface IWindowImpl : IDisposable
{
    Point Position { get; set; }

    Size Size { get; set; }

    string Title { get; set; }

    void PollEvents(Window window);

    void Show();

    void Hide();

    void Close();

    void SwapBuffers();
}
