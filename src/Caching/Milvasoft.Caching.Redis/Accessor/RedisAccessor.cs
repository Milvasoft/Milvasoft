using Milvasoft.Caching.Redis.Accessor;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// Provides redis cache operations.
/// </summary>
/// <remarks>
/// The injected <see cref="IConnectionMultiplexer"/> is shared (DI singleton) and owned by the container.
/// This accessor never closes, disposes or replaces it. StackExchange.Redis reconnects automatically after connection failures.
/// </remarks>
public partial class RedisAccessor : IRedisAccessor
{
    private readonly IConnectionMultiplexer _client;
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes new instance of <see cref="RedisAccessor"/> with the shared <paramref name="connectionMultiplexer"/>.
    /// </summary>
    /// <param name="connectionMultiplexer">Shared connection multiplexer. It is not owned by the accessor.</param>
    public RedisAccessor(IConnectionMultiplexer connectionMultiplexer)
    {
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);

        _client = connectionMultiplexer;
        _database = _client.GetDatabase();
    }

    /// <summary>
    /// Gets redis client connection state.
    /// </summary>
    /// <returns></returns>
    public bool IsConnected() => _client.IsConnected;

    /// <summary>
    /// Gets redis database.
    /// </summary>
    /// <returns></returns>
    public IDatabase GetDatabase(int db = -1, object asyncState = null) => _client.GetDatabase(db, asyncState);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    public IServer GetServer(string host, int port, object asyncState = null) => _client.GetServer(host, port, asyncState);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    public IServer GetServer(EndPoint endpoint, object asyncState = null) => _client.GetServer(endpoint, asyncState);

    #region Data Operations

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key) where T : class => ToObject<T>((string)_database.StringGet(key));

    /// <summary>
    /// Gets <paramref name="key"/>'s value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Get(string key) => _database.StringGet(key);

    /// <summary>
    /// Gets <paramref name="keys"/> values. Missing keys and values that cannot be deserialized to <typeparamref name="T"/> are skipped,
    /// so the result is not positionally aligned with <paramref name="keys"/>.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> Get<T>(IEnumerable<string> keys)
    {
        var redisKeys = ToRedisKeys(keys);

        if (redisKeys.Length == 0)
            return [];

        return DeserializeValues<T>(_database.StringGet(redisKeys));
    }

    /// <summary>
    /// Gets <paramref name="keys"/> values. Null or whitespace keys are ignored and duplicate keys are queried once.
    /// </summary>
    /// <returns></returns>
    public RedisValue[] Get(IEnumerable<string> keys)
    {
        var redisKeys = ToRedisKeys(keys);

        if (redisKeys.Length == 0)
            return [];

        return _database.StringGet(redisKeys) ?? [];
    }

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Set(string key, string value) => _database.StringSet(key, value);

    /// <summary>
    /// Sets <paramref name="value"/> with <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public bool Set<T>(string key, T value) where T : class => _database.StringSet(key, ToJson(value));

    /// <summary>
    /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiration">Relative time to live. If null, the key does not expire.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expiration"/> is zero or negative.</exception>
    public bool Set(string key, object value, TimeSpan? expiration)
    {
        if (!expiration.HasValue)
            return _database.StringSet(key, ToJson(value));

        return _database.StringSet(key, ToJson(value), new Expiration(EnsurePositive(expiration.Value)));
    }

    /// <summary>
    /// Removes <paramref name="key"/> and value.
    /// </summary>
    /// <param name="key"></param>
    public bool Remove(string key) => _database.KeyDelete(key);

    /// <summary>
    /// Checks if there is a <paramref name="key"/> in database.
    /// </summary>
    /// <param name="key"></param>
    public bool KeyExists(string key) => _database.KeyExists(key);

    /// <summary>
    /// Sets timeout on <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiration">Relative time to live. It is a duration and is never time zone converted.</param>
    /// <returns></returns>
    public bool KeyExpire(string key, TimeSpan expiration) => _database.KeyExpire(key, expiration);

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
    public bool KeyExpire(string key, DateTime? expiration) => _database.KeyExpire(key, EnsureKindSpecified(expiration));

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    public void Purge()
    {
        foreach (var server in GetConnectedPrimaryServers())
            server.FlushDatabase(_database.Database);
    }

    #endregion

    private IServer[] GetConnectedPrimaryServers()
    {
        var servers = _client.GetServers().Where(server => server.IsConnected && !server.IsReplica).ToArray();

        if (servers.Length == 0)
            throw new InvalidOperationException("There is no connected primary redis server.");

        return servers;
    }

    private static RedisKey[] ToRedisKeys(IEnumerable<string> keys)
    {
        if (keys == null)
            return [];

        return [.. keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().Select(k => (RedisKey)k)];
    }

    private static List<T> DeserializeValues<T>(RedisValue[] values)
    {
        var result = new List<T>(values?.Length ?? 0);

        if (values == null)
            return result;

        foreach (var value in values)
        {
            if (!value.HasValue)
                continue;

            try
            {
                var deserialized = JsonConvert.DeserializeObject<T>(value!);

                if (deserialized is not null)
                    result.Add(deserialized);
            }
            catch (JsonException)
            {
                // A cache entry that cannot be read as T is treated as a cache miss.
            }
        }

        return result;
    }

    private static TimeSpan EnsurePositive(TimeSpan expiration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiration, TimeSpan.Zero);

        return expiration;
    }

    private static DateTime? EnsureKindSpecified(DateTime? expiration)
    {
        if (expiration?.Kind == DateTimeKind.Unspecified)
            throw new ArgumentException("Expiration date kind must be Utc or Local.", nameof(expiration));

        return expiration;
    }
}
