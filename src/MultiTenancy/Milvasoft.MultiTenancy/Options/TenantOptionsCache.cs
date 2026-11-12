using Microsoft.Extensions.Options;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Accessor;
using System.Collections.Concurrent;

namespace Milvasoft.MultiTenancy.Options;

/// <summary>
/// Tenant aware options cache
/// </summary>
/// <typeparam name="TOptions"></typeparam>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <remarks>
/// Initializes new instance of <see cref="TenantOptionsCache{TOptions, TTenant, TKey}"/>
/// </remarks>
/// <param name="tenantAccessor"></param>
public class TenantOptionsCache<TOptions, TTenant, TKey>(ITenantAccessor<TTenant, TKey> tenantAccessor) : IOptionsMonitorCache<TOptions>
    where TOptions : class
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{

    private readonly ITenantAccessor<TTenant, TKey> _tenantAccessor = tenantAccessor;
    private readonly TenantOptionsCacheDictionary<TOptions, TKey> _tenantSpecificOptionsCache = new();

    /// <summary>
    /// Key of the current tenant's cache; requests without a tenant share the default key.
    /// </summary>
    private TKey CurrentTenantKey => _tenantAccessor.Tenant?.Id ?? default;

    /// <summary>
    /// Clears tenant options cache.
    /// </summary>
    public void Clear() => _tenantSpecificOptionsCache.Get(CurrentTenantKey).Clear();

    /// <summary>
    /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/>      
    /// if the key does not already exist. Returns the new value, or the existing value
    /// if the key exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="createOptions"></param>
    /// <returns></returns>
    public TOptions GetOrAdd(string name, Func<TOptions> createOptions) => _tenantSpecificOptionsCache.Get(CurrentTenantKey).GetOrAdd(name, createOptions);

    /// <summary>
    /// Tries to adds a new option to the cache, will return false if the name already exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public bool TryAdd(string name, TOptions options) => _tenantSpecificOptionsCache.Get(CurrentTenantKey).TryAdd(name, options);

    /// <summary>
    /// Try to remove an options instance.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool TryRemove(string name) => _tenantSpecificOptionsCache.Get(CurrentTenantKey).TryRemove(name);
}
