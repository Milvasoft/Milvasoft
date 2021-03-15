using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Caching.Redis
{
    /// <summary>
    /// Provides redis cache operations.
    /// </summary>
    public class RedisCacheService : IMilvaCacheService
    {
        private ConnectionMultiplexer _client;
        private readonly ConfigurationOptions _options;

        /// <summary>
        /// Initializes new instance of <see cref="RedisCacheService"/> with <paramref name="cacheServiceOptions"/>.
        /// </summary>
        /// <param name="cacheServiceOptions"></param>
        public RedisCacheService(RedisCacheServiceOptions cacheServiceOptions)
        {
            _options = cacheServiceOptions.ConfigurationOptions;

            if (cacheServiceOptions.ConnectWhenCreatingNewInstance)
                _client = ConnectionMultiplexer.Connect(_options);
        }

        /// <summary>
        /// Gets redis client connection state.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected() => _client?.IsConnected ?? false;

        #region Async

        /// <summary>
        /// Connects redis database.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync() => _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);

        /// <summary>
        /// Close all connections.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (_client != null && _client.IsConnected)
            {
                await _client.CloseAsync().ConfigureAwait(false);
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
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);
            return ((string)await _client.GetDatabase().StringGetAsync(key)).ToObject<T>();
        }

        /// <summary>
        /// Gets <paramref name="key"/>'s value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(string key)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);
            return await _client.GetDatabase().StringGetAsync(key);
        }

        /// <summary>
        /// Sets <paramref name="value"/> with <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, object value)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().StringSetAsync(key, value.ToJson());
        }

        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, object value, TimeSpan? expiration)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().StringSetAsync(key, value.ToJson(), expiration);
        }

        /// <summary>
        /// Removes <paramref name="key"/> and value.
        /// </summary>
        /// <param name="key"></param>
        public async Task<bool> RemoveAsync(string key)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().KeyDeleteAsync(key);
        }

        /// <summary>
        /// Checks if there is a <paramref name="key"/> in database. 
        /// </summary>
        /// <param name="key"></param>
        public async Task<bool> KeyExistsAsync(string key)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().KeyExistsAsync(key);
        }

        /// <summary>
        /// Sets timeout on <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task<bool> KeyExpireAsync(string key, TimeSpan expiration)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().KeyExpireAsync(key, expiration);
        }

        /// <summary>
        /// Sets timeout on <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task<bool> KeyExpireAsync(string key, DateTime? expiration)
        {
            await CheckClientAndConnectIfNotAsync().ConfigureAwait(false);

            return await _client.GetDatabase().KeyExpireAsync(key, expiration);
        }

        private async Task CheckClientAndConnectIfNotAsync()
        {
            if (_client == null)
            {
                _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);
            }
            else if (_client != null && !_client.IsConnected)
            {
                await _client.CloseAsync().ConfigureAwait(false);
                _client = await ConnectionMultiplexer.ConnectAsync(_options).ConfigureAwait(false);
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
        public T Get<T>(string key) where T : class => ((string)_client.GetDatabase().StringGet(key)).ToObject<T>();

        /// <summary>
        /// Gets <paramref name="key"/>'s value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => _client.GetDatabase().StringGet(key);

        /// <summary>
        /// Sets <paramref name="value"/> with <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Set(string key, string value) => _client.GetDatabase().StringSet(key, value);

        /// <summary>
        /// Sets <paramref name="value"/> with <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value) where T : class => _client.GetDatabase().StringSet(key, value.ToJson());

        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="key"/> with <paramref name="expiration"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public void Set(string key, object value, TimeSpan expiration) => _client.GetDatabase().StringSet(key, value.ToJson(), expiration);

        /// <summary>
        /// Removes <paramref name="key"/> and value.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key) => _client.GetDatabase().KeyDelete(key);

        /// <summary>
        /// Checks if there is a <paramref name="key"/> in database. 
        /// </summary>
        /// <param name="key"></param>
        public void KeyExists(string key) => _client.GetDatabase().KeyExists(key);

        #endregion
    }
}
