namespace Monosand.Win32;

public unsafe partial class Win32Platform : Platform
{
    private GraphicsBackend graphicsBackend;
    private MonosandPlatform identifier;

    public override GraphicsBackend GraphicsBackend => graphicsBackend;
    public override MonosandPlatform Identifier => identifier;

    public Win32Platform() { }
    internal override void Init()
    {
        // TODO error handle
        if (Interop.MsdInit() != 0)
            throw new OperationFailedException("Interop.MsdInit() return non-zero value.");
        graphicsBackend = Interop.MsdgGetGraphicsBackend();
        identifier = MonosandPlatform.Win32;
    }

    internal override WinImpl CreateWindowImpl(int width, int height, string title, Window window)
        => new Win32WinImpl(width, height, title, window);

    internal override IVertexBufferImpl CreateVertexBufferImpl(
        RenderContext context,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage
        )
        => new Win32VertexBufferImpl((Win32RenderContext)context, vertexDeclaration, dataUsage);

    internal override ITexture2DImpl CreateTexture2DImpl(RenderContext context, int width, int height)
        => new Win32Texture2DImpl((Win32RenderContext)context, width, height);

    // other api

    internal override Stream OpenReadStream(string fileName)
        => new FileStream(fileName, FileMode.Open, FileAccess.Read);

    internal unsafe override Span<byte> LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out int channels)
    {
        void* data;
        fixed (void* ptr = source)
        {
            data = Interop.MsdLoadImage(ptr, source.Length, out width, out height, out channels);
        }
        if (data is null)
            throw new ResourceLoadFailedException() { Type = ResourceType.Image };
        return new Span<byte>(data, width * height * channels);
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

    internal override unsafe IShaderImpl CreateShaderImplFromGlsl(RenderContext context, byte* vshSource, byte* fshSource)
        => Win32ShaderImpl.FromGlsl((Win32RenderContext)context, vshSource, fshSource);
}