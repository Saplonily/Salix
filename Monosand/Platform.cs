namespace Monosand;

public abstract class Platform
{
    /// <summary>Current platform.</summary>
    public abstract MonosandPlatform Identifier { get; }

    /// <summary>The graphics backend current using.</summary>
    public abstract GraphicsBackend GraphicsBackend { get; }

    /// <summary>Init this platform.</summary>
    internal abstract void Init();

    /// <summary>Create a <see cref="WinImpl"/>.</summary>
    internal abstract WinImpl CreateWindowImpl(int width, int height, string title, Window window);

    /// <summary>Create a <see cref="IVertexBufferImpl"/>.</summary>
    internal abstract IVertexBufferImpl CreateVertexBufferImpl(
        RenderContext context,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage,
        bool indexed
        );

    /// <summary>Create a <see cref="ITexture2DImpl"/>.</summary>
    internal abstract ITexture2DImpl CreateTexture2DImpl(RenderContext context, int width, int height);

    internal unsafe abstract IShaderImpl CreateShaderImplFromGlsl(RenderContext context, byte* vertSource, byte* fragSource);

    /// <summary>
    /// <para>Open a file stream to read.</para>
    /// <para>It returns a <see cref="Stream"/> instead of a <see cref="FileStream"/>
    /// that's because on some platforms we just can't return a <see cref="FileStream"/>. 
    /// So there's just <see cref="Stream"/>.</para>
    /// </summary>
    internal abstract Stream OpenReadStream(string fileName);

    /// <summary>Load an image from an image file memory.</summary>
    internal abstract Span<byte> LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format);

    /// <summary>Free the image read from the image file memory.</summary>
    internal abstract void FreeImage(Span<byte> image);

    internal abstract long GetUsecTimeline();
}