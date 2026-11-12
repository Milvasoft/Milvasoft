using StackExchange.Redis;
using System.Net;

namespace Milvasoft.Caching.Redis.Accessor;

/// <summary> 
/// Provides caching operations. It uses Newtonsoft.Json for serialization and deserialization.
/// </summary>
public interface IRedisAccessor : ICacheAccessor<RedisAccessor>
{
    #region Async

    /// <summary>
    /// Gets redis database.
    /// </summary>
    /// <returns></returns>
    IDatabase GetDatabase(int db = -1, object asyncState = null);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    IServer GetServer(string host, int port, object asyncState = null);

    /// <summary>
    /// Gets redis server.
    /// </summary>
    /// <returns></returns>
    IServer GetServer(EndPoint endpoint, object asyncState = null);

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    Task<RedisValue[]> GetAsync(IEnumerable<string> keys);

    /// <summary>
    /// Sets <paramref name="values"/>.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    Task<bool> SetAsync(KeyValuePair<RedisKey, RedisValue>[] values);

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server. Same as <see cref="PurgeAsync"/>.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    Task FlushDatabaseAsync();

    /// <summary>
    /// It performs the requested redis action.
    /// If redis is not reachable (not connected, connection or timeout error), it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Error logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    Task PerformRedisActionAsync(Func<Task> action,
                                 string userFriendlyMessageLocalizerKey,
                                 IMilvaLogger milvaLogger = null);

    /// <summary>
    /// It performs the requested redis action.
    /// If redis is not reachable (not connected, connection or timeout error), it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key.
    /// Error logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    Task<T> PerformRedisActionAsync<T>(Func<Task<T>> action,
                                       string userFriendlyMessageLocalizerKey,
                                       IMilvaLogger milvaLogger = null);

    /// <summary>
    /// Kept for backward compatibility. It does nothing.
    /// </summary>
    /// <remarks>
    /// The shared <see cref="IConnectionMultiplexer"/> is never closed or replaced by the accessor.
    /// StackExchange.Redis reconnects automatically in the background; use <see cref="ICacheAccessor.IsConnected"/> to check the current state.
    /// </remarks>
    /// <returns></returns>
    ValueTask CheckClientAndConnectIfNotAsync();

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    Task PurgeAsync();

    #endregion

    #region Sync

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public RedisValue[] Get(IEnumerable<string> keys);

    /// <summary>
    /// Removes all keys of the accessor's database on every connected primary server.
    /// The multiplexer must be configured with <see cref="ConfigurationOptions.AllowAdmin"/> at startup.
    /// </summary>
    /// <returns></returns>
    void Purge();

    #endregion
}
