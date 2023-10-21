namespace Monosand;

public abstract class Platform
{
    /// <summary>Current platform.</summary>
    public abstract MonosandPlatform Identifier { get; }

    /// <summary>The graphics backend currently using.</summary>
    public abstract GraphicsBackend GraphicsBackend { get; }

    /// <summary>Initialize this platform.</summary>
    internal abstract void Initialize();

    internal abstract WindowImpl CreateWindowImpl(int width, int height, string title, Window window);

    internal abstract RenderContext CreateRenderContext();

    internal abstract void AttachRenderContext(RenderContext context, Window window);

    internal abstract Stream OpenReadStream(string fileName);

    internal abstract Span<byte> LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format);

    internal abstract void FreeImage(Span<byte> image);

    internal abstract long GetUsecTimeline();
}