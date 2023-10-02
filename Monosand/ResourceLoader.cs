using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Monosand;

public class ResourceLoader
{
    private readonly Game game;
    private readonly Platform platform;

    internal ResourceLoader(Game game)
        => (this.game, platform) = (game, game.Platform);

    public Stream OpenReadStream(string fileName)
        => platform.OpenReadStream(fileName);

    public Texture2D LoadTexture2D(Stream stream)
    {
        unsafe
        {
            var fs = stream;
            long lengthl = fs.Length;
            if (lengthl > int.MaxValue)
                throw new NotSupportedException("The stream is too long.");
            int length = (int)lengthl;
            void* mem = Marshal.AllocHGlobal(length).ToPointer();
            fs.Read(new Span<byte>(mem, length));
            

            var img = platform.LoadImage(new ReadOnlySpan<byte>(mem, length), out int width, out int height, out int channels);
            // support rgba just for now
            Debug.Assert(channels == 4);
            Texture2D tex = new(width, height, img);
            platform.FreeImage(img);
            Marshal.FreeHGlobal((nint)mem);
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