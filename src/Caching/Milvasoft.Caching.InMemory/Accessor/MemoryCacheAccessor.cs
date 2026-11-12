using Microsoft.Extensions.Caching.Memory;

namespace Milvasoft.Caching.InMemory.Accessor;

/// <summary>
/// <see cref="IMemoryCache"/> based cache accessor.
/// </summary>
/// <remarks>
/// Every entry is stored with <see cref="MemoryCacheEntryOptions.Size"/> = 1, so <see cref="MemoryCacheOptions.SizeLimit"/> limits the entry count.
/// </remarks>
/// <param name="cache"></param>
public class MemoryCacheAccessor(IMemoryCache cache) : IMemoryCacheAccessor
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    /// <inheritdoc/>
    public bool IsConnected() => true;

    #region Get

    /// <inheritdoc/>
    public T Get<T>(string key) where T : class => _cache.TryGetValue(key, out var value) ? value as T : null;

    /// <inheritdoc/>
    public string Get(string key) => Get<string>(key);

    /// <summary>
    /// Gets <paramref name="keys"/> values. Missing keys and values that are not <typeparamref name="T"/> are skipped.
    /// </summary>
    public IEnumerable<T> Get<T>(IEnumerable<string> keys)
    {
        if (keys == null)
            return [];

        var values = new List<T>();

        foreach (var key in keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct())
            if (_cache.TryGetValue(key, out var value) && value is T typedValue)
                values.Add(typedValue);

        return values;
    }

    /// <summary>
    /// Gets <paramref name="key"/>'s value if it is an instance of <paramref name="returnType"/>; otherwise returns null.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    public object Get(string key, Type returnType)
    {
        ArgumentNullException.ThrowIfNull(returnType);

        return _cache.TryGetValue(key, out var value) && returnType.IsInstanceOfType(value) ? value : null;
    }

    /// <inheritdoc/>
    public Task<T> GetAsync<T>(string key) where T : class => Task.FromResult(Get<T>(key));

    /// <inheritdoc/>
    public Task<object> GetAsync(string key, Type returnType) => Task.FromResult(Get(key, returnType));

    /// <inheritdoc/>
    public Task<string> GetAsync(string key) => Task.FromResult(Get(key));

    /// <inheritdoc/>
    public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys) => Task.FromResult(Get<T>(keys));

    #endregion

    #region Set

    /// <inheritdoc/>
    public bool Set(string key, string value) => Set(key, (object)value, null);

    /// <inheritdoc/>
    public bool Set<T>(string key, T value) where T : class => Set(key, value, null);

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration">Relative time to live. If null, the entry does not expire.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expiration"/> is zero or negative.</exception>
    public bool Set(string key, object value, TimeSpan? expiration)
    {
        var options = CreateEntryOptions();

        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration.Value;

        _cache.Set(key, value, options);

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> SetAsync(string key, object value) => Task.FromResult(Set(key, value, null));

    /// <inheritdoc/>
    public Task<bool> SetAsync(string key, object value, TimeSpan? expiration) => Task.FromResult(Set(key, value, expiration));

    #endregion

    #region Key operations

    /// <inheritdoc/>
    public bool KeyExists(string key) => _cache.TryGetValue(key, out _);

    /// <inheritdoc/>
    public Task<bool> KeyExistsAsync(string key) => Task.FromResult(KeyExists(key));

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration">Relative time to live. Zero or negative removes the entry (same as redis).</param>
    /// <returns>True if the key exists.</returns>
    public bool KeyExpire(string key, TimeSpan expiration)
    {
        if (!_cache.TryGetValue(key, out var item))
            return false;

        if (expiration <= TimeSpan.Zero)
        {
            _cache.Remove(key);

            return true;
        }

        var options = CreateEntryOptions();

        options.AbsoluteExpirationRelativeToNow = expiration;

        _cache.Set(key, item, options);

        return true;
    }

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration">
    /// Absolute expiration time. Its <see cref="DateTime.Kind"/> must be <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.
    /// If null, the existing timeout is removed and the entry becomes persistent (same as redis PERSIST).
    /// </param>
    /// <returns>True if the key exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expiration"/> kind is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public bool KeyExpire(string key, DateTime? expiration)
    {
        if (expiration?.Kind == DateTimeKind.Unspecified)
            throw new ArgumentException("Expiration date kind must be Utc or Local.", nameof(expiration));

        if (!_cache.TryGetValue(key, out var item))
            return false;

        var options = CreateEntryOptions();

        if (expiration.HasValue)
            options.AbsoluteExpiration = new DateTimeOffset(expiration.Value);

        _cache.Set(key, item, options);

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration) => Task.FromResult(KeyExpire(key, expiration));

    /// <inheritdoc/>
    public Task<bool> KeyExpireAsync(string key, DateTime? expiration) => Task.FromResult(KeyExpire(key, expiration));

    #endregion

    #region Remove

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        _cache.Remove(key);

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(string key) => Task.FromResult(Remove(key));

    /// <summary>
    /// Removes <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns>The number of keys that existed and were removed.</returns>
    public Task<long> RemoveAsync(IEnumerable<string> keys)
    {
        long removedCount = 0;

        foreach (var key in keys ?? [])
        {
            if (_cache.TryGetValue(key, out _))
                removedCount++;

            _cache.Remove(key);
        }

        return Task.FromResult(removedCount);
    }

    #endregion

    private static MemoryCacheEntryOptions CreateEntryOptions() => new() { Size = 1 };
}
