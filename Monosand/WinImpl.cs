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

    // Rendering

    /// <summary>Swap the buffers. (see: DoubleBuffered)</summary>
    internal abstract void SwapBuffers();

    /// <summary>Set the viewport of this window.</summary>
    internal abstract void SetViewport(int x, int y, int width, int height);

    internal abstract void Clear(Color color);

    internal abstract void DrawPrimitives<T>(
        VertexDeclaration vertexDeclaration, PrimitiveType primitiveType,
        T[] vertices
        ) where T : unmanaged;
}