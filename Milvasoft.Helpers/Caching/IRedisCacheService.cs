using System;
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
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Close all connections.
        /// If connection exists, closes the connection.
        /// If connection not exists, disposes client object.
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

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
