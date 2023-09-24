using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Monosand;

public class ResourceLoader
{
    private readonly Game game;

    internal ResourceLoader(Game game)
    {
        this.game = game;
    }

    public Stream OpenReadStream(string fileName)
        => game.platform.OpenReadStream(fileName);

    public Texture2D LoadTexture2D(string fileName)
    {
        unsafe
        {
            using var fs = OpenReadStream(fileName);
            var length = fs.Length;
            void* mem = Marshal.AllocHGlobal((nint)length).ToPointer();
            fs.Read(new Span<byte>(mem, (int)length));

            var img = game.platform.LoadImage(new ReadOnlySpan<byte>(mem, (int)length), out int width, out int height, out int channels);
            // support rgba just for now
            Debug.Assert(channels == 4);
            Texture2D tex = new(width, height, img);
            game.platform.FreeImage(img);
            Marshal.FreeHGlobal((nint)mem);
            return tex;
        }
    }
}