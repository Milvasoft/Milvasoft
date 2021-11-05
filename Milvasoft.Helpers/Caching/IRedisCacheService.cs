using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Exceptions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Caching
{
    /// <summary>
    /// Provides caching operations.
    /// </summary>
    public interface IRedisCacheService
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
        /// Gets <paramref name="keys"/> values.
        /// </summary>
        /// <returns></returns>
        Task<RedisValue[]> GetAsync(IEnumerable<string> keys);

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
        /// Sets <paramref name="value"/> with <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void Set(string key, string value);

        /// <summary>
        /// Sets <paramref name="value"/> with <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set<T>(string key, T value) where T : class;

        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        void Set(string key, object value, TimeSpan expiration);

        /// <summary>
        /// Removes <paramref name="key"/> and value.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

        /// <summary>
        /// Checks if there is a <paramref name="key"/> in database. 
        /// </summary>
        /// <param name="key"></param>
        void KeyExists(string key);

        #endregion
    }
}
