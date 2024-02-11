using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Exceptions;
using StackExchange.Redis;
using System.Net;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// Provides caching operations.
/// </summary>
public interface IRedisAccessor : ICacheAccessor<RedisAccessor>
{
    /// <summary>
    /// Gets redis client connection state.
    /// </summary>
    /// <returns></returns>
    bool IsConnected();

    #region Async

    /// <summary>
    /// Connects redis database if there is no connection. Otherwise this method does nothing.
    /// </summary>
    /// <returns> Returns connected status. </returns>
    ValueTask<bool> ConnectAsync();

    /// <summary>
    /// Close all connections.
    /// If connection exists, closes the connection.
    /// If connection not exists, disposes client object.
    /// </summary>
    /// <returns></returns>
    ValueTask DisconnectAsync();

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
    /// Flushs default database.
    /// </summary>
    /// <returns></returns>
    Task FlushDatabaseAsync();

    /// <summary>
    /// It performs the requested redis action in try catch blocks. If redis client not connected, connects.
    /// If an error occurs when performing action or connecting to redis, it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key. 
    /// Fatal logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    Task PerformRedisActionAsync(Func<Task> action,
                                 string userFriendlyMessageLocalizerKey,
                                 IMilvaLogger milvaLogger = null);

    /// <summary>
    /// It performs the requested redis action in try catch blocks. If redis client not connected, connects.
    /// If an error occurs when performing action or connecting to redis, it throws the <see cref="MilvaUserFriendlyException"/> error along with the message key. 
    /// Fatal logging if <paramref name="milvaLogger"/> object is not null.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="userFriendlyMessageLocalizerKey"></param>
    /// <param name="milvaLogger"></param>
    /// <returns></returns>
    Task<T> PerformRedisActionAsync<T>(Func<Task<T>> action,
                                       string userFriendlyMessageLocalizerKey,
                                       IMilvaLogger milvaLogger = null);

    /// <summary>
    /// Checks redis connection and if connection close try to open connection.
    /// </summary>
    /// <returns></returns>
    ValueTask CheckClientAndConnectIfNotAsync();

    #endregion

    #region Sync

    /// <summary>
    /// Connects redis database if there is no connection. Otherwise this method does nothing.
    /// </summary>
    /// <returns></returns>
    public void Connect();

    /// <summary>
    /// Close all connections.
    /// If connection exists, closes the connection.
    /// If connection not exists, disposes client object.
    /// </summary>
    /// <returns></returns>
    public void Disconnect();

    /// <summary>
    /// Gets <paramref name="keys"/> values.
    /// </summary>
    /// <returns></returns>
    public RedisValue[] Get(IEnumerable<string> keys);

    #endregion
}
