using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Caching;

/// <summary>
/// Provides redis cache operations. Redis connection multiplexer must be singleton.
/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private IConnectionMultiplexer _client;
    private readonly ConfigurationOptions _options;
    private readonly bool _useUtcForDateTimes;
    private IDatabase _database;

    /// <summary>
    /// Initializes new instance of <see cref="RedisCacheService"/> with <paramref name="cacheServiceOptions"/>.
    /// </summary>
    /// <param name="cacheServiceOptions"></param>
    /// <param name="connectionMultiplexer"></param>
    public RedisCacheService(RedisCacheServiceOptions cacheServiceOptions, IConnectionMultiplexer connectionMultiplexer)
    {
        _options = cacheServiceOptions.ConfigurationOptions;
        _useUtcForDateTimes = cacheServiceOptions.UseUtcForExpirationDates;
        _client = connectionMultiplexer;
        _database = _client?.GetDatabase();
    }

    /// <summary>
    /// Gets redis client connection state.
    /// </summary>
    /// <returns></returns>
    public bool IsConnected() => _client?.IsConnected ?? false;

    /// <summary>
    /// Gets redis database.
    /// </summary>
    /// <returns></returns>
    public IDatabase GetDatabase(int db = -1, object asyncState = null) => _client?.GetDatabase(db, asyncState);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    public IServer GetServer(string host, int port, object asyncState = null) => _client?.GetServer(host, port, asyncState);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    public IServer GetServer(EndPoint endpoint, object asyncState = null) => _client?.GetServer(endpoint, asyncState);

    #region Async

    /// <summary>
    /// Connects redis database if there is no connection. Otherwise this method does nothing.
    /// </summary>
    /// <returns> Returns connected status. </returns>
    public async ValueTask<bool> ConnectAsync()
    {
        if (!IsConnected())
            _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);
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
            await _client.CloseAsync().ConfigureAwait(false);
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
        => ((string)await _database.StringGetAsync(key).ConfigureAwait(false)).ToObject<T>();

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string> GetAsync(string key)
        => await _database.StringGetAsync(key).ConfigureAwait(false);

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys)
    {
        if (keys.IsNullOrEmpty())
            return null;

        keys.ToList().RemoveAll(i => string.IsNullOrWhiteSpace(i));

        var redisKeys = Array.ConvertAll(keys.ToArray(), item => (RedisKey)item);

        var values = await _database.StringGetAsync(redisKeys).ConfigureAwait(false);

        if (values.IsNullOrEmpty())
            return null;

        var stringValues = values.ToStringArray();

        List<T> redisValues = new();

        foreach (var item in stringValues)
        {
            if (!string.IsNullOrWhiteSpace(item))
                redisValues.Add(JsonConvert.DeserializeObject<T>(item));
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

        keys.ToList().RemoveAll(i => string.IsNullOrWhiteSpace(i));

        var redisKeys = Array.ConvertAll(keys.ToArray(), item => (RedisKey)item);

        var values = await _database.StringGetAsync(redisKeys).ConfigureAwait(false);

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
    public async Task<bool> SetAsync(string key, object value)
        => await _database.StringSetAsync(key, value.ToJson()).ConfigureAwait(false);

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync(string key, object value, TimeSpan? expiration)
        => await _database.StringSetAsync(key, value.ToJson(), expiration.HasValue
                                                               ? _useUtcForDateTimes
                                                                    ? expiration.Value.ConvertToUtc()
                                                                    : expiration
                                                               : expiration).ConfigureAwait(false);

    /// <summary>
    /// Sets <paramref name="values"/>.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync(KeyValuePair<RedisKey, RedisValue>[] values)
        => await _database.StringSetAsync(values).ConfigureAwait(false);

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    public async Task<bool> RemoveAsync(string key)
        => await _database.KeyDeleteAsync(key).ConfigureAwait(false);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database. 
    /// </summary>
    /// <param name="key"></param>
    public async Task<bool> KeyExistsAsync(string key)
        => await _database.KeyExistsAsync(key).ConfigureAwait(false);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public async Task<bool> KeyExpireAsync(string key, TimeSpan expiration)
        => await _database.KeyExpireAsync(key, _useUtcForDateTimes ? expiration.ConvertToUtc() : expiration).ConfigureAwait(false);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public async Task<bool> KeyExpireAsync(string key, DateTime? expiration)
        => await _database.KeyExpireAsync(key, expiration).ConfigureAwait(false);

    /// <summary>
    /// Flushs default database.
    /// </summary>
    /// <returns></returns>
    public async Task FlushDatabaseAsync()
    {
        _options.AllowAdmin = true;

        await DisconnectAsync().ConfigureAwait(false);

        var client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);

        try
        {
            if (client?.IsConnected ?? false)
                await client.GetServer(client.GetEndPoints().FirstOrDefault()).FlushDatabaseAsync().ConfigureAwait(false);

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
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            if (IsConnected())
            {
                await action().ConfigureAwait(false);
            }
            else throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey);
        }
        catch (MilvaUserFriendlyException)
        {
            if (milvaLogger != null)
                _ = milvaLogger.LogFatalAsync("Cannot reach redis server.", MailSubject.Error);

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
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            if (IsConnected())
            {
                return await action().ConfigureAwait(false);
            }
            else throw new MilvaUserFriendlyException(userFriendlyMessageLocalizerKey);
        }
        catch (MilvaUserFriendlyException)
        {
            if (milvaLogger != null)
                _ = milvaLogger.LogFatalAsync("Cannot reach redis server.", MailSubject.Error);

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
            _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);
            _database = _client.GetDatabase();
        }
        else if (_client != null && !_client.IsConnected)
        {
            await _client.CloseAsync().ConfigureAwait(false);
            _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);
            _database = _client.GetDatabase();
        }
    }

    #endregion

    #region Sync

    /// <summary>
    /// Connects redis database.
    /// </summary>
    /// <returns></returns>
    public void Connect() => _client = ConnectionMultiplexer.Connect(_options);

    /// <summary>
    /// Close all connections.
    /// </summary>
    /// <returns></returns>
    public void Disconnect()
    {
        if (_client != null && _client.IsConnected)
        {
            _client.Close();
        }
        else if (_client != null && !_client.IsConnected)
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
    public T Get<T>(string key) where T : class => ((string)_database.StringGet(key)).ToObject<T>();

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Get(string key) => _database.StringGet(key);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public void Set(string key, string value) => _database.StringSet(key, value);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set<T>(string key, T value) where T : class => _database.StringSet(key, value.ToJson());

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public void Set(string key, object value, TimeSpan expiration) => _database.StringSet(key, value.ToJson(), _useUtcForDateTimes ? expiration.ConvertToUtc() : expiration);

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key) => _database.KeyDelete(key);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database. 
    /// </summary>
    /// <param name="key"></param>
    public void KeyExists(string key) => _database.KeyExists(key);

    #endregion
}
