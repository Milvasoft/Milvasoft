using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Store
{
    /// <summary>
    /// Cached tenant store.
    /// </summary>
    public class InMemoryTenantStore<TTenant, TKey> : ITenantStore<TTenant, TKey>
        where TKey : IEquatable<TKey>
        where TTenant : class, IMilvaTenantBase<TKey>, new()
    {
        private readonly IRedisCacheService _redisCacheService;

        /// <summary>
        /// Creates new instance of <see cref="InMemoryTenantStore{TTenant, TKey}"/>
        /// </summary>
        /// <param name="redisCacheService"></param>
        public InMemoryTenantStore(IRedisCacheService redisCacheService)
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
            await EnsureConnected().ConfigureAwait(false);

            return await _redisCacheService.GetAsync<TTenant>(identifier.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets a tenant with a given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public async Task<bool> SetTenantAsync(TKey identifier, TTenant tenant)
        {
            await EnsureConnected().ConfigureAwait(false);

            return await _redisCacheService.SetAsync(identifier.ToString(), tenant).ConfigureAwait(false);
        }

        private async Task EnsureConnected()
        {
            if (!_redisCacheService.IsConnected())
            {
                try
                {
                    await _redisCacheService.ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw new MilvaDeveloperException("Cannot reach redis server.");
                }
            }
        }
    }
}
