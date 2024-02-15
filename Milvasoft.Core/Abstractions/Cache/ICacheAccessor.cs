namespace Milvasoft.Core.Abstractions.Cache;

public interface ICacheAccessor
{
    /// <summary>
    /// Gets redis client connection state.
    /// </summary>
    /// <returns></returns>
    bool IsConnected();

    #region Async

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<string> GetAsync(string key);

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<bool> SetAsync(string key, object value);

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    Task<bool> SetAsync(string key, object value, TimeSpan? expiration);

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Removes <paramref name="keys"/> and value.
    /// </summary>
    /// <param name="keys"></param>
    Task<long> RemoveAsync(IEnumerable<string> keys);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database. 
    /// </summary>
    /// <param name="key"></param>
    Task<bool> KeyExistsAsync(string key);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    Task<bool> KeyExpireAsync(string key, TimeSpan expiration);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    Task<bool> KeyExpireAsync(string key, DateTime? expiration);

    #endregion

    #region Sync

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T Get<T>(string key) where T : class;

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string Get(string key);

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    IEnumerable<T> Get<T>(IEnumerable<string> keys);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Set(string key, string value);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    bool Set<T>(string key, T value) where T : class;

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    bool Set(string key, object value, TimeSpan? expiration);

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    bool Remove(string key);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database. 
    /// </summary>
    /// <param name="key"></param>
    bool KeyExists(string key);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    bool KeyExpire(string key, TimeSpan expiration);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    bool KeyExpire(string key, DateTime? expiration);

    #endregion
}

public interface ICacheAccessor<TAccessor> : ICacheAccessor where TAccessor : class, ICacheAccessor
{
}
