using System.Drawing;

namespace Monosand;

public abstract class Platform
{
    /// <summary>Init this platform.</summary>
    internal abstract void Init();

    /// <summary>
    /// Create a <see cref="WinImpl"/>.
    /// </summary>
    /// <param name="width">Window width</param>
    /// <param name="height">Window height</param>
    /// <param name="title">Window title</param>
    /// <param name="window">The <see cref="Window"/> attached to</param>
    /// <returns></returns>
    internal abstract WinImpl CreateWindowImpl(int width, int height, string title, Window window);

    internal abstract VertexBufferImpl CreateVertexBufferImpl(
        WinImpl winImpl, 
        VertexDeclaration vertexDeclaration, 
        VertexBufferDataUsage dataUsage
        );
}