using System.Drawing;

namespace Monosand;

internal unsafe abstract class WinImpl
{
    /// <summary>Destroy this window and release the resource.</summary>
    internal abstract void Destroy();

    /// <summary>Pool and dispatch events like PointerClick, PointerMove etc.</summary>
    internal abstract void PollEvents();

    /// <summary>Show this window. Automatically called by <see cref="Game.Run"/>.</summary>
    internal abstract void Show();

    /// <summary>Hide this window.</summary>
    internal abstract void Hide();

    /// <summary>Get the position of this window.</summary>
    internal abstract Point GetPosition();

    /// <summary>Get the size of this window.</summary>
    internal abstract Size GetSize();

    /// <summary>Set the position of this window.</summary>
    internal abstract void SetPosition(int x, int y);

    /// <summary>Set the size of this window.</summary>
    internal abstract void SetSize(int width, int height);

    /// <summary>Get the RenderContext of this window.</summary>
    internal abstract RenderContext GetRenderContext();

    internal abstract void MainThreadDispose();
}