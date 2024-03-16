using System.Reflection;
using ByteArrayPool = System.Buffers.ArrayPool<byte>;

namespace Monosand;

public sealed class ResourceLoader
{
    private readonly Game game;
    private readonly Platform platform;

    public ResourceLoader(Game game)
    {
        this.game = game;
        platform = game.Platform;
    }

    private static int VerifyLongValue(long value)
    {
        if (value is >= int.MaxValue)
            throw new NotSupportedException(SR.StreamIsTooLong);
        if (value is < 0)
            throw new ArgumentException(SR.InvalidStreamLength, nameof(value));
        return (int)value;
    }


    public SpriteFont LoadSpriteFont(Stream streamTexture, Stream streamEntries)
    {
        ThrowHelper.ThrowIfNull(streamTexture);
        ThrowHelper.ThrowIfNull(streamEntries);
        Texture2D tex = LoadTexture2D(streamTexture);
        BinaryReader br = new(streamEntries);
        Dictionary<char, SpriteFont.CharEntry> dic = new();
        short fontSize = br.ReadInt16();
        int count = checked((int)streamEntries.Length - 1) / (sizeof(char) + 7 * sizeof(short));
        for (int i = 0; i < count; i++)
        {
            SpriteFont.CharEntry entry = new();
            char c = (char)br.ReadInt16();
            entry.X = br.ReadInt16();
            entry.Y = br.ReadInt16();
            entry.Width = br.ReadInt16();
            entry.Height = br.ReadInt16();
            entry.BearingX = br.ReadInt16();
            entry.BearingY = br.ReadInt16();
            entry.Advance = br.ReadInt16();
            dic[c] = entry;
        }
        return new(tex, fontSize, dic);
    }

    public unsafe Texture2D LoadTexture2D(Stream stream)
    {
        ThrowHelper.ThrowIfNull(stream);
        int length = VerifyLongValue(stream.Length);
        byte[] bytes = ByteArrayPool.Shared.Rent(length);
        stream.Read(bytes, 0, length);

        var chunk = platform.LoadImage(new ReadOnlySpan<byte>(bytes, 0, length), out int width, out int height, out ImageFormat format);
        if (chunk.IsEmpty)
            throw new ArgumentException(SR.InvalidImageData, nameof(stream));
        Texture2D texture = new(game.RenderContext, width, height, chunk.Pointer, format);
        platform.FreeImage(chunk);

        ByteArrayPool.Shared.Return(bytes);
        return texture;
    }

    public Shader LoadGlslShader(Stream vertStream, Stream fragStream)
    {
        ThrowHelper.ThrowIfNull(vertStream);
        ThrowHelper.ThrowIfNull(fragStream);
        int vertLength = VerifyLongValue(vertStream.Length + 1);
        int fragLength = VerifyLongValue(fragStream.Length + 1);
        byte[] vertData = ByteArrayPool.Shared.Rent(vertLength);
        byte[] fragData = ByteArrayPool.Shared.Rent(fragLength);
        vertStream.Read(vertData, 0, vertLength - 1);
        fragStream.Read(fragData, 0, fragLength - 1);
        vertData[vertLength - 1] = 0;
        fragData[fragLength - 1] = 0;

        var context = game.RenderContext;
        Shader? shader;
        shader = new Shader(context, new ReadOnlySpan<byte>(vertData, 0, vertLength), new(fragData, 0, fragLength));

        ByteArrayPool.Shared.Return(vertData);
        ByteArrayPool.Shared.Return(fragData);
        return shader;
    }

    public static Stream OpenEmbeddedFileStream(string file, Assembly? assembly = null)
        => (assembly ?? Assembly.GetExecutingAssembly()).GetManifestResourceStream(file)
            ?? throw new FileNotFoundException(null, file);
}