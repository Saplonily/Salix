namespace Monosand.Win32;

public unsafe partial class Win32Platform : Platform
{

    private GraphicsBackend graphicsBackend;
    private MonosandPlatform identifier;

    public override GraphicsBackend GraphicsBackend => graphicsBackend;
    public override MonosandPlatform Identifier => identifier;

    public Win32Platform() { }

    internal override void Initialize()
    {
        // TODO error handle
        if (Interop.MsdInitialize() != 0)
            throw new OperationFailedException("MsdInitialize returned non-zero value.");
        graphicsBackend = Interop.MsdgGetGraphicsBackend();
        identifier = MonosandPlatform.Win32;
    }

    internal override WindowImpl CreateWindowImpl(int width, int height, string title, Window window)
        => new Win32WinImpl(width, height, title, window);

    internal override RenderContext CreateRenderContext()
        => new Win32RenderContext();

    internal override Stream OpenReadStream(string fileName)
        => new FileStream(fileName, FileMode.Open, FileAccess.Read);

    internal unsafe override Span<byte> LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format)
    {
        void* data;
        fixed (void* ptr = source)
        {
            data = Interop.MsdLoadImage(ptr, source.Length, out width, out height, out int size, out format);
            if (data is null)
                throw new ResourceLoadFailedException() { Type = ResourceType.Image };
            return new Span<byte>(data, size);
        }
    }

    internal override void FreeImage(Span<byte> image)
    {
        if (image.IsEmpty)
            throw new ArgumentException("'Span<byte> image' is null.", nameof(image));

        fixed (void* ptr = image)
        {
            Interop.MsdFreeImage(ptr);
        }
    }

    internal override long GetUsecTimeline()
        => Interop.MsdGetUsecTimeline();

    internal override void AttachRenderContext(RenderContext context, Window window)
    {
        ((Win32RenderContext)context).AttachTo(window);
    }
}