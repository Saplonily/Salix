namespace Saladim.Salix;

public abstract class Platform
{
    /// <summary>The graphics backend currently using.</summary>
    public GraphicsBackend GraphicsBackend { get; protected set; }

    /// <summary>Current platform.</summary>
    public SalixPlatform Identifier { get; protected set; }

    #region resource loading

    internal abstract UnmanagedMemory LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format);

    internal abstract void FreeImage(UnmanagedMemory imageData);

    #endregion

    #region impl

    internal abstract IWindowImpl CreateWindowImpl(int width, int height, string title);

    internal abstract IRenderContextImpl CreateRenderContextImpl(IWindowImpl windowImpl);

    #endregion
}