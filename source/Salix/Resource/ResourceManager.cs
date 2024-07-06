using System.Runtime.CompilerServices;

namespace Saladim.Salix;

public abstract class ResourceManager<TToken> where TToken : notnull
{
    private readonly WeakDictionary<TToken, object> weakDictionary;
    protected readonly ResourceLoader loader;

    protected ResourceManager(ResourceLoader loader)
    {
        weakDictionary = new();
        this.loader = loader;
    }

    public T Load<T>(TToken token) where T : class
    {
        if (weakDictionary.TryGetValue(token, out object? value) && (value is not IResource ires || !ires.IsDisposed))
        {
            if (value is not T)
                throw new ResourceLoadFailedException(typeof(T));
            return Unsafe.As<T>(value);
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