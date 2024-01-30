﻿using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core;
using Milvasoft.Core.Extensions;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// Provides redis cache operations. Redis connection multiplexer must be singleton.
/// </summary>
public partial class RedisAccessor : IRedisAccessor
{
    private IConnectionMultiplexer _client;
    private readonly ConfigurationOptions _options;
    private readonly bool _useUtcForDateTimes;
    private IDatabase _database;

    /// <summary>
    /// Initializes new instance of <see cref="RedisAccessor"/> with <paramref name="cacheServiceOptions"/>.
    /// </summary>
    /// <param name="cacheServiceOptions"></param>
    /// <param name="connectionMultiplexer"></param>
    public RedisAccessor(RedisCachingOptions cacheServiceOptions, IConnectionMultiplexer connectionMultiplexer)
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

    #region Data Operations

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
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> Get<T>(IEnumerable<string> keys)
    {
        if (keys.IsNullOrEmpty())
            return null;

        keys.ToList().RemoveAll(i => string.IsNullOrWhiteSpace(i));

        var redisKeys = Array.ConvertAll(keys.ToArray(), item => (RedisKey)item);

        var values = _database.StringGet(redisKeys);

        if (values.IsNullOrEmpty())
            return null;

        var stringValues = values.ToStringArray();

        List<T> redisValues = [];

        foreach (var item in stringValues)
        {
            if (!string.IsNullOrWhiteSpace(item))
                redisValues.Add(JsonSerializer.Deserialize<T>(item));
        }

        return redisValues;
    }

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public RedisValue[] Get(IEnumerable<string> keys)
    {
        if (keys.IsNullOrEmpty())
            return null;

        keys.ToList().RemoveAll(i => string.IsNullOrWhiteSpace(i));

        var redisKeys = Array.ConvertAll(keys.ToArray(), item => (RedisKey)item);

        var values = _database.StringGet(redisKeys);

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