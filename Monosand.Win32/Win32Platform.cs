namespace Monosand.Win32;

public unsafe partial class Win32Platform : Platform
{
    private static bool inited = false;
    public Win32Platform() { }

    internal override void Init()
    {
        if (!inited)
        {
            // TODO error handle
            Win32WinImpl.InitMsgCallbacks();
            inited = true;
            int a = Interop.MsdInit();
            if (a != 0)
                throw new OperationFailedException("Interop.MsdInit() return non-zero value.");
        }
    }

    internal override WinImpl CreateWindowImpl(int width, int height, string title, Window window)
        => new Win32WinImpl(width, height, title, window);

    internal override VertexBufferImpl CreateVertexBufferImpl(WinImpl winImpl, VertexDeclaration vertexDeclaration, VertexBufferDataUsage dataUsage)
        => new Win32VertexBufferImpl(winImpl, vertexDeclaration, dataUsage);

    internal override Texture2DImpl CreateTexture2DImpl(WinImpl winImpl, int width, int height)
        => new Win32Texture2DImpl(winImpl, width, height);

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
            throw new ArgumentException("Span<byte> image is null.", nameof(image));

        fixed (void* ptr = image)
        {
            Interop.MsdFreeImage(ptr);
        }
    }
}