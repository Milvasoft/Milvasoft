using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Milvasoft.Helpers.MultiTenancy.Options
{
    /// <summary>
    /// Dictionary of tenant specific options caches.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class TenantOptionsCacheDictionary<TOptions, TKey> where TOptions : class where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Caches stored in memory.
        /// </summary>
        private readonly ConcurrentDictionary<TKey, IOptionsMonitorCache<TOptions>> _tenantSpecificOptionCaches = new();

        /// <summary>
        /// Gets options for specific tenant (create if not exists).
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public IOptionsMonitorCache<TOptions> Get(TKey tenantId)
        {
            return _tenantSpecificOptionCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>());
        }
    }
}
