using System.Reflection;

namespace Monosand;

// TODO caching
public class ResourceLoader
{
    private readonly Game game;
    private readonly Platform platform;

    internal ResourceLoader(Game game)
        => (this.game, platform) = (game, game.Platform);

    public Stream OpenReadStream(string fileName)
        => platform.OpenReadStream(fileName);

    public Stream OpenEmbeddedStream(string fileName, Assembly? assembly = null)
        => (assembly ?? Assembly.GetExecutingAssembly()).GetManifestResourceStream(fileName)
            ?? throw new FileNotFoundException(null, fileName);

    public SpriteFont LoadSpriteFont(Stream streamTexture, Stream entriesBin)
    {
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

    // TODO, use NativeMemory.Alloc to make GC more happy
    public Texture2D LoadTexture2D(Stream stream)
    {
        unsafe
        {
            var fs = stream;
            int length = fs.Length > int.MaxValue ?
                throw new NotSupportedException("Stream is too long.")
                : (int)fs.Length;

            byte[] bytes = new byte[length];
            fs.Read(bytes, 0, length);

            var img = platform.LoadImage(new ReadOnlySpan<byte>(bytes, 0, length), out int width, out int height, out ImageFormat format);

            Texture2D tex = new(game.RenderContext, width, height, img, format);
            platform.FreeImage(img);
            return tex;
        }
    }

    public Shader LoadGlslShader(Stream vertStream, Stream fragStream)
    {
        byte[] vsh = ReadAllBytes(vertStream);
        byte[] fsh = ReadAllBytes(fragStream);
        unsafe
        {
            var context = game.RenderContext;
            var impl = context.CreateGlslShaderImpl(new(vsh), new(fsh));
            return new(context, impl);
        }

        static byte[] ReadAllBytes(Stream stream)
        {
            long length = stream.Length;
            if (length > int.MaxValue)
                throw new NotSupportedException("The stream is too long.");
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }
    }

    public Texture2D LoadTexture2D(string fileName)
    {
        using var fs = OpenReadStream(fileName);
        return LoadTexture2D(fs);
    }

    public Shader LoadGlslShader(string vshFileName, string fshFileName)
    {
        using var vshfs = OpenReadStream(vshFileName);
        using var fshfs = OpenReadStream(fshFileName);
        return LoadGlslShader(vshfs, fshfs);
    }

    public SpriteFont LoadSpriteFont(string textureFileName, string entriesBinFileName)
    {
        using var ts = OpenReadStream(textureFileName);
        using var es = OpenReadStream(entriesBinFileName);
        return LoadSpriteFont(ts, es);
    }
}