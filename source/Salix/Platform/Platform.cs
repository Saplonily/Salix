namespace Salix;

#pragma warning disable CA1822 // Mark members as static

// TODO maybe we can just remove this class
public unsafe class Platform
{
    private GraphicsBackend graphicsBackend;
    private SalixPlatform identifier;

    /// <summary>The graphics backend currently using.</summary>
    public GraphicsBackend GraphicsBackend => graphicsBackend;

    /// <summary>Current platform.</summary>
    public SalixPlatform Identifier => identifier;

    public Platform() { }

    internal void Initialize()
    {
        if (Interop.SLX_Initialize())
            throw new FrameworkException(SR.PlatformInitializeFailed, Interop.SLX_GetError());
        graphicsBackend = GraphicsBackend.Opengl33;
        identifier = SalixPlatform.Win32;
    }

    // TODO remove these resource loading methods
    internal unsafe UnmanagedMemory LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format)
    {
        void* data;
        fixed (void* ptr = source)
        {
            data = Interop.SLX_LoadImage(ptr, source.Length, out width, out height, out int size, out format);
            if (data is null) return UnmanagedMemory.Empty;
            return new(data, size);
        }
    }

    internal void FreeImage(UnmanagedMemory imageData)
    {
        if (imageData.IsEmpty)
            throw new ArgumentException(SR.ImageDataIsNull, nameof(imageData));
        Interop.SLX_FreeImage(imageData.Pointer);
    }
}