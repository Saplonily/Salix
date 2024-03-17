using System.Diagnostics.CodeAnalysis;

namespace Monosand;

internal sealed class WeakDictionary<TKey, TValue>
    where TKey : notnull where TValue : class
{
    private readonly Dictionary<TKey, WeakReference<TValue>> dictionary;

    public WeakDictionary() => dictionary = new();

    public void Add(TKey key, TValue value)
        => dictionary.Add(key, new(value));

    public bool Remove(TKey key)
        => dictionary.Remove(key);

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        if (!dictionary.TryGetValue(key, out var weakRef))
        {
            value = null;
            return false;
        }
        return weakRef.TryGetTarget(out value);
    }

    public void Clean()
    {
        // see https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.remove?view=net-8.0
        /*
            .NET Core 3.0+ only: 
            this mutating method may be safely called without invalidating 
            active enumerators on the Dictionary<TKey,TValue> instance. 
            This does not imply thread safety.
        */
#if NETCOREAPP3_1_OR_GREATER
        foreach (var pair in dictionary)
            if (!pair.Value.TryGetTarget(out _))
                dictionary.Remove(pair.Key);
#else
        List<TKey> keys = new();
        foreach (var pair in dictionary)
            if (!pair.Value.TryGetTarget(out _))
                keys.Add(pair.Key);
        foreach (var key in keys)
            dictionary.Remove(key);
#endif
    }

    public bool ContainsKey(TKey key)
    {
        bool r = dictionary.TryGetValue(key, out var wref);
        return r && wref!.TryGetTarget(out _);
    }

    public TValue this[TKey key]
    {
        get
        {
            bool r = dictionary.TryGetValue(key, out var wref);
            if (!r) throw new KeyNotFoundException(string.Format(SR.KeyNotPresent, key));
            if (wref!.TryGetTarget(out var target))
                return target;
            else
                throw new KeyNotFoundException(string.Format(SR.KeyNotPresentButAlive, key));
        }
        set
        {
            dictionary[key] = new(value);
        }
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var item in dictionary)
        {
            if (item.Value?.TryGetTarget(out var target) is true)
                yield return new(item.Key, target);
        }
    }
}