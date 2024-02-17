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
public class TenantOptionsCache<TOptions, TTenant, TKey> : IOptionsMonitorCache<TOptions>
    where TOptions : class
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{

    private readonly ITenantAccessor<TTenant, TKey> _tenantAccessor;
    private readonly TenantOptionsCacheDictionary<TOptions, TKey> _tenantSpecificOptionsCache = new TenantOptionsCacheDictionary<TOptions, TKey>();

    /// <summary>
    /// Initializes new instance of <see cref="TenantOptionsCache{TOptions, TTenant, TKey}"/>
    /// </summary>
    /// <param name="tenantAccessor"></param>
    public TenantOptionsCache(ITenantAccessor<TTenant, TKey> tenantAccessor)
    {
        _tenantAccessor = tenantAccessor;
    }

    /// <summary>
    /// Clears tenant options cache.
    /// </summary>
    public void Clear()
    {
        _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id).Clear();
    }

    /// <summary>
    /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/>      
    /// if the key does not already exist. Returns the new value, or the existing value
    /// if the key exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="createOptions"></param>
    /// <returns></returns>
    public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
    {
        return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
            .GetOrAdd(name, createOptions);
    }

    /// <summary>
    /// Tries to adds a new option to the cache, will return false if the name already exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public bool TryAdd(string name, TOptions options)
    {
        return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
            .TryAdd(name, options);
    }

    /// <summary>
    /// Try to remove an options instance.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool TryRemove(string name)
    {
        return _tenantSpecificOptionsCache.Get(_tenantAccessor.Tenant.Id)
            .TryRemove(name);
    }
}
