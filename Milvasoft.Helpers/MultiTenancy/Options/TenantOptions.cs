using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Milvasoft.Helpers.MultiTenancy.Options
{
    /// <summary>
    /// Makes IOptions tenant aware.
    /// </summary>
    public class TenantOptions<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IOptionsMonitorCache<TOptions> _cache;

        /// <summary>
        /// Initializes new instance of <see cref="TenantOptions{TOptions}"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="cache"></param>
        public TenantOptions(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
        {
            _factory = factory;
            _cache = cache;
        }

        /// <summary>
        /// Options Value.
        /// </summary>
        public TOptions Value => Get("Default");

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/>      
        /// if the key does not already exist. Returns the new value, or the existing value
        /// if the key exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TOptions Get(string name)
        {
            return _cache.GetOrAdd(name, () => _factory.Create(name));
        }
    }
}
