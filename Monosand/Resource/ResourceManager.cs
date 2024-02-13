#pragma warning disable CA1822

using System.Reflection;

namespace Monosand;

public abstract class ResourceManager<TToken> where TToken : notnull
{
    private readonly WeakDictionary<TToken, object> weakDictionary;
    protected readonly ResourceLoader loader;

    public ResourceManager(ResourceLoader loader)
    {
        weakDictionary = new();
        this.loader = loader;
    }

    public T Load<T>(TToken token) where T : class
    {
        if (weakDictionary.TryGetValue(token, out object? value))
        {
            if (value is null or not T)
                throw new ResourceLoadFailedException(typeof(T));
            return (T)value;
        }
        else
        {
            var res = LoadByToken<T>(token);
            weakDictionary[token] = res;
            return res;
        }
    }

    internal void Clean()
        => weakDictionary.Clean();

    protected abstract T LoadByToken<T>(TToken path) where T : class;
}

public unsafe class FileSystemResourceManager : ResourceManager<string>
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
        throw new NotSupportedException($"Resoure type {typeof(T)} is not supported.");
    }
}