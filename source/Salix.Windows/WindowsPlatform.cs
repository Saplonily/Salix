namespace Saladim.Salix.Windows;

public sealed class WindowsPlatform : Platform
{
    public WindowsPlatform()
    {
        if (Interop.SLX_Initialize())
            Interop.Throw(SR.PlatformInitializeFailed);
    }

    internal override unsafe UnmanagedMemory LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format)
    {
        void* data;
        int size;
        fixed (void* ptr = source)
            data = Interop.SLX_LoadImage(ptr, source.Length, out width, out height, out size, out format);
        if (data is null) return UnmanagedMemory.Empty;
        return new UnmanagedMemory(data, size);
    }

    internal override unsafe void FreeImage(UnmanagedMemory imageData)
    {
        if (imageData.IsEmpty)
            throw new ArgumentNullException(nameof(imageData), SR.ImageDataIsNull);
        Interop.SLX_FreeImage(imageData.Pointer);
    }

    internal override IRenderContextImpl CreateRenderContextImpl(IWindowImpl windowImpl)
        => new RenderContextImpl(windowImpl);

    internal override IWindowImpl CreateWindowImpl(int width, int height, string title)
        => new WindowImpl(width, height, title);
}
