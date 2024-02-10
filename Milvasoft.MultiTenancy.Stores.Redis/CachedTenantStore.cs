using Milvasoft.Caching.Redis;
using Milvasoft.Core.EntityBase.MultiTenancy;
using Milvasoft.MultiTenancy.Store;

namespace Milvasoft.MultiTenancy.Stores.Redis;

/// <summary>
/// Cached tenant store. Sample store for <see cref="ITenantStore{TTenant, TKey}"/>
/// </summary>
public class CachedTenantStore<TTenant, TKey> : ITenantStore<TTenant, TKey>
    where TKey : struct, IEquatable<TKey>
    where TTenant : class, IMilvaTenantBase<TKey>, new()
{
    private readonly IRedisAccessor _redisCacheService;

    /// <summary>
    /// Creates new instance of <see cref="CachedTenantStore{TTenant, TKey}"/>
    /// </summary>
    /// <param name="redisCacheService"></param>
    public CachedTenantStore(IRedisAccessor redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    /// <summary>
    /// Returns a tenant according to <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<TTenant> GetTenantAsync(TKey identifier)
    {
        await _redisCacheService.CheckClientAndConnectIfNotAsync();

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
        await _redisCacheService.CheckClientAndConnectIfNotAsync();

        if (_redisCacheService.IsConnected())
            return await _redisCacheService.SetAsync(identifier.ToString(), tenant).ConfigureAwait(false);
        else return false;
    }
}
