namespace Application.Interfaces;

public interface IMemoryStorage
{
    void Set<T>(string key, T value);
    T Get<T>(string key);
    bool Remove(string key);
    bool ContainsKey(string key);
}