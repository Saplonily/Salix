namespace Saladim.Salix;

public sealed unsafe class FileSystemResourceManager : ResourceManager<string>
{
    public string RootPath { get; set; }

    public FileSystemResourceManager(ResourceLoader loader, string rootPath = "Content")
        : base(loader)
        => RootPath = rootPath;

    public Stream OpenReadStream(string path)
        => new FileStream(Path.Combine(RootPath, path), FileMode.Open, FileAccess.Read);

    protected override T LoadByToken<T>(string token) where T : class
    {
        string path = token;
        if (typeof(T) == typeof(SpriteFont))
        {
            using Stream fst = OpenReadStream(path + ".png");
            using Stream fse = OpenReadStream(path + ".bin");
            return (T)(object)loader.LoadSpriteFont(fst, fse);
        }
        if (typeof(T) == typeof(Texture2D))
        {
            using Stream fs = OpenReadStream(path + ".png");
            return (T)(object)loader.LoadTexture2D(fs);
        }
        if (typeof(T) == typeof(Shader))
        {
            using Stream v = OpenReadStream(path + ".vert");
            using Stream f = OpenReadStream(path + ".frag");
            return (T)(object)loader.LoadGlslShader(v, f);
        }
        throw new NotSupportedException(string.Format(SR.ResourceTypeNotSupported,typeof(T)));
    }
}