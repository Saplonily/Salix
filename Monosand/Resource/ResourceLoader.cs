using System.Diagnostics;

namespace Monosand;

public class ResourceLoader
{
    private readonly Game game;
    private readonly Platform platform;

    internal ResourceLoader(Game game)
        => (this.game, platform) = (game, game.Platform);

    public Stream OpenReadStream(string fileName)
        => platform.OpenReadStream(fileName);

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

            var img = platform.LoadImage(new ReadOnlySpan<byte>(bytes, 0, length), out int width, out int height, out int channels);

            // support rgba only, just for now
            Debug.Assert(channels == 4);
            Texture2D tex = new(width, height, img);
            platform.FreeImage(img);
            return tex;
        }
    }

    public Shader LoadGlslShader(Stream vshStream, Stream fshStream)
    {
        byte[] vsh = ReadAllBytes(vshStream);
        byte[] fsh = ReadAllBytes(fshStream);
        unsafe
        {
            fixed (byte* vshPtr = vsh)
            {
                fixed (byte* fshPtr = fsh)
                {
                    var context = game.Window.RenderContext;
                    var impl = platform.CreateShaderImplFromGlsl(context, vshPtr, fshPtr);
                    Shader shader = new(context, impl);
                    return shader;
                }
            }
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
}