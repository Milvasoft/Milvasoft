using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace Milvasoft.Caching.InMemory.Accessor;

/// <inheritdoc/>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class MemoryCacheAccessor(IMemoryCache cache) : IMemoryCacheAccessor
{
    private readonly IMemoryCache _cache = cache;
    private static readonly MethodInfo _genericGetMethod = Array.Find(typeof(MemoryCacheAccessor).GetMethods(), i => i.Name == nameof(Get) && i.IsGenericMethod);

    public bool IsConnected() => _cache != null;

    public T Get<T>(string key) where T : class => _cache?.Get<T>(key);

    public string Get(string key) => _cache?.Get<string>(key);

    public IEnumerable<T> Get<T>(IEnumerable<string> keys) => throw new NotImplementedException();

    public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys) => throw new NotImplementedException();

    public Task<T> GetAsync<T>(string key) where T : class => Task.Run(() => Get<T>(key));

    public Task<object> GetAsync(string key, Type returnType) => Task.Run(() => _genericGetMethod.MakeGenericMethod(returnType).Invoke(this, [key]));

    public Task<string> GetAsync(string key) => Task.Run(() => Get(key));

    public bool KeyExists(string key) => _cache.TryGetValue(key, out _);

    public Task<bool> KeyExistsAsync(string key) => Task.Run(() => KeyExists(key));

    public bool KeyExpire(string key, TimeSpan expiration)
    {
        var item = _cache.Get(key);

        if (item != null)
            return _cache.Set(key, item, expiration) != null;

        return false;
    }

    public bool KeyExpire(string key, DateTime? expiration)
    {
        var item = _cache.Get(key);

        if (item != null && expiration.HasValue)
            return _cache.Set(key, item, expiration.Value) != null;

        return false;
    }

    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration) => Task.Run(() => KeyExpire(key, expiration));

    public Task<bool> KeyExpireAsync(string key, DateTime? expiration) => Task.Run(() => KeyExpire(key, expiration));

    public bool Remove(string key)
    {
        _cache.Remove(key);

        return true;
    }

    public Task<bool> RemoveAsync(string key) => Task.Run(() => Remove(key));

    public Task<long> RemoveAsync(IEnumerable<string> keys) => throw new NotImplementedException();

    public bool Set(string key, string value) => _cache.Set(key, value) != null;

    public bool Set<T>(string key, T value) where T : class => _cache.Set(key, value) != null;

    public bool Set(string key, object value, TimeSpan? expiration) => (expiration.HasValue ? _cache.Set(key, value, expiration.Value) : Set(key, value)) != null;

    public Task<bool> SetAsync(string key, object value) => Task.Run(() => Set(key, value));

    public Task<bool> SetAsync(string key, object value, TimeSpan? expiration) => Task.Run(() => Set(key, value, expiration));
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
