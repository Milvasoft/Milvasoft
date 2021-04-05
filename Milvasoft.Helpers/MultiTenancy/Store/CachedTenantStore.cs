using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Store
{
    /// <summary>
    /// Cached tenant store.
    /// </summary>
    public class CachedTenantStore<TTenant, TKey> : ITenantStore<TTenant, TKey>
        where TKey : struct, IEquatable<TKey>
        where TTenant : class, IMilvaTenantBase<TKey>, new()
    {
        private readonly IRedisCacheService _redisCacheService;

        /// <summary>
        /// Creates new instance of <see cref="CachedTenantStore{TTenant, TKey}"/>
        /// </summary>
        /// <param name="redisCacheService"></param>
        public CachedTenantStore(IRedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
            Task.WaitAll(_redisCacheService.ConnectAsync());
        }
        /// <summary>
        /// Returns a tenant according to <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<TTenant> GetTenantAsync(TKey identifier)
        {
            if (_redisCacheService.IsConnected())
                return await _redisCacheService.GetAsync<TTenant>(identifier.ToString()).ConfigureAwait(false);
            else return null;
        }

        /// <summary>
        /// Sets a tenant with a given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public async Task<bool> SetTenantAsync(TKey identifier, TTenant tenant)
        {
            if (_redisCacheService.IsConnected())
                return await _redisCacheService.SetAsync(identifier.ToString(), tenant).ConfigureAwait(false);
            else return false;
        }

        private async Task<bool> EnsureConnected()
        {
            if (!_redisCacheService.IsConnected())
            {
                try
                {
                    return await _redisCacheService.ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else return true;
        }
    }
}
