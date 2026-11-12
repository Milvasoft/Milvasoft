using Newtonsoft.Json;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// Provides redis cache operations.
/// </summary>
public partial class RedisAccessor
{
    private const string _cannotReachRedisMessage = "Cannot reach redis server.";

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string key) where T : class
        => ToObject<T>((string)await _database.StringGetAsync(key).ConfigureAwait(false));

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    public async Task<object> GetAsync(string key, Type returnType)
        => ToObject((string)await _database.StringGetAsync(key).ConfigureAwait(false), returnType);

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string> GetAsync(string key)
        => await _database.StringGetAsync(key).ConfigureAwait(false);

    /// <summary>
    /// Gets <paramref name="keys"/> values. Missing keys and values that cannot be deserialized to <typeparamref name="T"/> are skipped,
    /// so the result is not positionally aligned with <paramref name="keys"/>.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
    {
        var redisKeys = ToRedisKeys(keys);

        if (redisKeys.Length == 0)
            return [];

        var values = await _database.StringGetAsync(redisKeys).ConfigureAwait(false);

        return DeserializeValues<T>(values);
    }

    /// <summary>
    /// Gets <paramref name="keys"/> values. Null or whitespace keys are ignored and duplicate keys are queried once.
    /// Returns an empty array when there is nothing to query.
    /// </summary>
    /// <returns></returns>
    public async Task<RedisValue[]> GetAsync(IEnumerable<string> keys)
    {
        var redisKeys = ToRedisKeys(keys);

        if (redisKeys.Length == 0)
            return [];

        return await _database.StringGetAsync(redisKeys).ConfigureAwait(false) ?? [];
    }

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task<bool> SetAsync(string key, object value)
        => _database.StringSetAsync(key, ToJson(value));

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration">Relative time to live. It is a duration and is never time zone converted. If null, the key does not expire.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expiration"/> is zero or negative.</exception>
    public Task<bool> SetAsync(string key, object value, TimeSpan? expiration)
    {
        if (!expiration.HasValue)
            return _database.StringSetAsync(key, ToJson(value));

        return _database.StringSetAsync(key, ToJson(value), new Expiration(EnsurePositive(expiration.Value)));
    }

    /// <summary>
    /// Sets <paramref name="values"/>.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public Task<bool> SetAsync(KeyValuePair<RedisKey, RedisValue>[] values)
        => _database.StringSetAsync(values);

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    public Task<bool> RemoveAsync(string key)
        => _database.KeyDeleteAsync(key);

    /// <summary>
    /// Removes <paramref name="keys"/> and value.
    /// </summary>
    /// <param name="keys"></param>
    public Task<long> RemoveAsync(IEnumerable<string> keys)
        => _database.KeyDeleteAsync(keys: [.. keys.Select(i => new RedisKey(i))]);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database.
    /// </summary>
    /// <param name="key"></param>
    public Task<bool> KeyExistsAsync(string key)
        => _database.KeyExistsAsync(key);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration">Relative time to live. It is a duration and is never time zone converted.</param>
    /// <returns></returns>
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration)
        => _database.KeyExpireAsync(key, expiration);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration">
    /// Absolute expiration time. Its <see cref="DateTime.Kind"/> must be <see cref="DateTimeKind.Utc"/> or <see cref="DateTimeKind.Local"/>.
    /// If null, the existing timeout is removed and the key becomes persistent.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expiration"/> kind is <see cref="DateTimeKind.Unspecified"/>.</exception>
    public Task<bool> KeyExpireAsync(string key, DateTime? expiration)
        => _database.KeyExpireAsync(key, EnsureKindSpecified(expiration));

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server. Same as <see cref="PurgeAsync"/>.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    public Task FlushDatabaseAsync() => PurgeAsync();

    /// <summary>
    /// It performs the requested redis action.
    /// If redis is not reachable (not connected, connection or timeout error), it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Error logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    public Task PerformRedisActionAsync(Func<Task> action, string userFriendlyMessageLocalizerKey, IMilvaLogger milvaLogger = null)
        => PerformRedisActionAsync(async () =>
        {
            await action().ConfigureAwait(false);

            return true;
        }, userFriendlyMessageLocalizerKey, milvaLogger);

    /// <summary>
    /// It performs the requested redis action.
    /// If redis is not reachable (not connected, connection or timeout error), it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Error logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    public async Task<T> PerformRedisActionAsync<T>(Func<Task<T>> action, string userFriendlyMessageLocalizerKey, IMilvaLogger milvaLogger = null)
    {
        if (!IsConnected())
        {
            milvaLogger?.Error(_cannotReachRedisMessage);

            throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey);
        }

        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException)
        {
            milvaLogger?.Error(ex, _cannotReachRedisMessage);

            throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey, ex);
        }
    }

    /// <summary>
    /// Kept for backward compatibility. It does nothing.
    /// </summary>
    /// <remarks>
    /// The shared <see cref="IConnectionMultiplexer"/> is never closed or replaced by the accessor.
    /// StackExchange.Redis reconnects automatically in the background; use <see cref="IsConnected"/> to check the current state.
    /// </remarks>
    /// <returns></returns>
    public ValueTask CheckClientAndConnectIfNotAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    public async Task PurgeAsync()
    {
        foreach (var server in GetConnectedPrimaryServers())
            await server.FlushDatabaseAsync(_database.Database).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts an object to a json string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string ToJson(object value) => JsonConvert.SerializeObject(value);

    /// <summary>
    /// Converts json string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="jsonOptions">The json deserialization options (optional).</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T ToObject<T>(string value, JsonSerializerSettings jsonOptions = null) where T : class
        => string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<T>(value, jsonOptions);

    /// <summary>
    /// Converts a string to an object of the specified return type.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="returnType">The return type of the object to convert to.</param>
    /// <param name="jsonOptions">The json deserialization options (optional).</param>
    /// <returns>The deserialized object of the specified return type.</returns>
    public static object ToObject(string value, Type returnType, JsonSerializerSettings jsonOptions = null)
        => string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject(value, returnType, jsonOptions);
}
