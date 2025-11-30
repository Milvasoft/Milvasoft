using Newtonsoft.Json;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// Provides redis cache operations. Redis connection multiplexer must be singleton.
/// </summary>
public partial class RedisAccessor
{
    /// <summary>
    /// Connects redis database if there is no connection. Otherwise this method does nothing.
    /// </summary>
    /// <returns> Returns connected status. </returns>
    public async ValueTask<bool> ConnectAsync()
    {
        if (!IsConnected())
            _client = await ConnectionMultiplexer.ConnectAsync(_options);
        return IsConnected();
    }

    /// <summary>
    /// Close all connections.
    /// If connection exists, closes the connection.
    /// If connection not exists, disposes client object.
    /// Do not use this method when application is running. Redis connection multiplexer must be singleton.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisconnectAsync()
    {
        if (_client != null && IsConnected())
        {
            await _client.CloseAsync();
        }
        else if (_client != null && !IsConnected())
        {
            _client.Dispose();
        }
    }

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
        => await _database.StringGetAsync(key);

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
    {
        if (keys.IsNullOrEmpty())
            return [];

        var filteredKeys = keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToArray();

        if (filteredKeys.Length == 0)
            return [];

        var redisKeys = Array.ConvertAll(filteredKeys, item => (RedisKey)item);

        var values = await _database.StringGetAsync(redisKeys);

        if (values.Length == 0 || Array.TrueForAll(values, v => !v.HasValue))
            return [];

        var redisValues = new List<T>(values.Length);

        foreach (var value in values)
        {
            if (!value.HasValue)
                continue;

            try
            {
                var deserialized = JsonConvert.DeserializeObject<T>(value!);

                if (deserialized is not null)
                    redisValues.Add(deserialized);
            }
            catch
            {
                // Handle deserialization error.
            }
        }

        return redisValues;
    }

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public async Task<RedisValue[]> GetAsync(IEnumerable<string> keys)
    {
        if (keys.IsNullOrEmpty())
            return null;

        var filteredKeys = keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToArray();

        if (filteredKeys.Length == 0)
            return [];

        var redisKeys = Array.ConvertAll(filteredKeys, item => (RedisKey)item);

        var values = await _database.StringGetAsync(redisKeys);

        if (values.IsNullOrEmpty())
            return null;

        return values;
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
    /// <param name="expiration"></param>
    /// <returns></returns>
    public Task<bool> SetAsync(string key, object value, TimeSpan? expiration)
    {
        if (expiration.HasValue)
            return _database.StringSetAsync(key, ToJson(value), new Expiration(_useUtcForDateTimes ? expiration.Value.ConvertToUtc() : expiration.Value));
        else
            return _database.StringSetAsync(key, ToJson(value));
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
    /// <param name="expiration"></param>
    /// <returns></returns>
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration)
        => _database.KeyExpireAsync(key, _useUtcForDateTimes ? expiration.ConvertToUtc() : expiration);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public Task<bool> KeyExpireAsync(string key, DateTime? expiration)
        => _database.KeyExpireAsync(key, expiration);

    /// <summary>
    /// Flushs default database.
    /// </summary>
    /// <returns></returns>
    public async Task FlushDatabaseAsync()
    {
        _options.AllowAdmin = true;

        await DisconnectAsync();

        var client = await ConnectionMultiplexer.ConnectAsync(_options);

        try
        {
            if (client?.IsConnected ?? false)
                await client.GetServer(client.GetEndPoints().FirstOrDefault()).FlushDatabaseAsync();

            _options.AllowAdmin = false;
        }
        catch (Exception)
        {
            throw new MilvaUserFriendlyException("RedisError");
        }
    }

    /// <summary>
    /// It performs the requested redis action in try catch blocks. If redis client not connected, connects.
    /// If an error occurs when performing action or connecting to redis, it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Fatal logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    public async Task PerformRedisActionAsync(Func<Task> action, string userFriendlyMessageLocalizerKey, IMilvaLogger milvaLogger = null)
    {
        try
        {
            await CheckClientAndConnectIfNotAsync();

            if (IsConnected())
            {
                await action();
            }
            else
                throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey);
        }
        catch (MilvaUserFriendlyException)
        {
            milvaLogger?.Error("Cannot reach redis server.");

            throw;
        }
    }

    /// <summary>
    /// It performs the requested redis action in try catch blocks. If redis client not connected, connects.
    /// If an error occurs when performing action or connecting to redis, it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Fatal logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    public async Task<T> PerformRedisActionAsync<T>(Func<Task<T>> action, string userFriendlyMessageLocalizerKey, IMilvaLogger milvaLogger = null)
    {
        try
        {
            await CheckClientAndConnectIfNotAsync();

            if (IsConnected())
            {
                return await action();
            }
            else
                throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey);
        }
        catch (MilvaUserFriendlyException)
        {
            milvaLogger?.Error("Cannot reach redis server.");

            throw;
        }
    }

    /// <summary>
    /// Checks redis connection and if connection close try to open connection.
    /// </summary>
    /// <returns></returns>
    public async ValueTask CheckClientAndConnectIfNotAsync()
    {
        if (_client == null)
        {
            _client = await ConnectionMultiplexer.ConnectAsync(_options);
            _database = _client.GetDatabase();
        }
        else if (!_client.IsConnected)
        {
            await _client.CloseAsync();
            _client = await ConnectionMultiplexer.ConnectAsync(_options);
            _database = _client.GetDatabase();
        }
    }

    /// <summary>
    /// Removes current database.
    /// </summary>
    /// <returns></returns>
    public async Task PurgeAsync()
    {
        _options.AllowAdmin = true;

        await _client.GetServer(_options.EndPoints[0]).FlushDatabaseAsync(_database.Database);

        _options.AllowAdmin = false;
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
