using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ByteArrayPool = System.Buffers.ArrayPool<byte>;

#pragma warning disable CA1822 // Mark members as static

namespace Monosand;

// TODO caching
public unsafe class ResourceLoader
{
    private readonly Game game;
    private readonly Platform platform;

    internal ResourceLoader(Game game)
        => (this.game, platform) = (game, game.Platform);

    private static int MakeItNotTooLong(long value)
        => value is > int.MaxValue or < 0 ? throw new NotSupportedException("The stream is too long.") : (int)value;

    public Stream OpenReadStream(string fileName)
        => platform.OpenReadStream(fileName);

    public Stream OpenEmbeddedStream(string fileName, Assembly? assembly = null)
        => (assembly ?? Assembly.GetExecutingAssembly()).GetManifestResourceStream(fileName)
            ?? throw new FileNotFoundException(null, fileName);

    public SpriteFont LoadSpriteFont(Stream streamTexture, Stream entriesBin)
    {
        ThrowHelper.ThrowIfNull(streamTexture);
        ThrowHelper.ThrowIfNull(entriesBin);
        Texture2D tex = LoadTexture2D(streamTexture);
        BinaryReader br = new(entriesBin);
        Dictionary<char, SpriteFont.CharEntry> dic = new();
        short fontSize = br.ReadInt16();
        int count = checked((int)entriesBin.Length - 1) / (sizeof(char) + 7 * sizeof(short));
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

    public Texture2D LoadTexture2D(Stream stream)
    {
        ThrowHelper.ThrowIfNull(stream);
        int length = MakeItNotTooLong(stream.Length);
        byte[] bytes = ByteArrayPool.Shared.Rent(length);
        stream.Read(bytes, 0, length);

        var chunk = platform.LoadImage(new ReadOnlySpan<byte>(bytes, 0, length), out int width, out int height, out ImageFormat format);
        Texture2D texture = new(game.RenderContext, width, height, chunk.Pointer, format);
        platform.FreeImage(chunk);

        ByteArrayPool.Shared.Return(bytes);
        return texture;
    }

    public Shader LoadGlslShader(Stream vertStream, Stream fragStream)
    {
        ThrowHelper.ThrowIfNull(vertStream);
        ThrowHelper.ThrowIfNull(fragStream);
        int vertLength = MakeItNotTooLong(vertStream.Length + 1);
        int fragLength = MakeItNotTooLong(fragStream.Length + 1);
        byte[] vertData = ByteArrayPool.Shared.Rent(vertLength);
        byte[] fragData = ByteArrayPool.Shared.Rent(fragLength);
        vertStream.Read(vertData, 0, vertLength - 1);
        fragStream.Read(fragData, 0, fragLength - 1);
        vertData[vertLength - 1] = 0;
        fragData[fragLength - 1] = 0;

        var context = game.RenderContext;
        var shader = new Shader(context, new ReadOnlySpan<byte>(vertData, 0, vertLength), new(fragData, 0, fragLength));

        ByteArrayPool.Shared.Return(vertData);
        ByteArrayPool.Shared.Return(fragData);
        return shader;
    }

    public Texture2D LoadTexture2D(string fileName)
    {
        using var fs = OpenReadStream(fileName);
        return LoadTexture2D(fs);
    }

    public Shader LoadGlslShader(string vertFileName, string fragFileName)
    {
        using var vshfs = OpenReadStream(vertFileName);
        using var fshfs = OpenReadStream(fragFileName);
        return LoadGlslShader(vshfs, fshfs);
    }

    public SpriteFont LoadSpriteFont(string textureFileName, string entriesBinFileName)
    {
        using var ts = OpenReadStream(textureFileName);
        using var es = OpenReadStream(entriesBinFileName);
        return LoadSpriteFont(ts, es);
    }
}