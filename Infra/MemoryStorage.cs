using System.Collections.Concurrent;
using Application.Interfaces;

namespace Infra;

public class MemoryStorage : IMemoryStorage
{
    private readonly ConcurrentDictionary<string, object> _storage = new();

    public void Set<T>(string key, T value)
    {
        _storage[key] = value!;
    }

    public T Get<T>(string key)
    {
        return _storage.TryGetValue(key, out var value) ? (T)value : default!;
    }

    public bool Remove(string key)
    {
        return _storage.TryRemove(key, out _);
    }

    public bool ContainsKey(string key)
    {
        return _storage.ContainsKey(key);
    }
}