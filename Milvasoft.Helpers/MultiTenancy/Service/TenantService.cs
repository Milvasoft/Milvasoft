using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.ResolutionStrategy;
using Milvasoft.Helpers.MultiTenancy.Store;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Service;

/// <summary>
/// Tenant access service.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class TenantService<TTenant, TKey> : ITenantService<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    private readonly ITenantResolutionStrategy<TKey> _tenantResolutionStrategy;
    private readonly ITenantStore<TTenant, TKey> _tenantStore;

    /// <summary>
    /// Creates new instances of <see cref="TenantService{TTenant, TKey}"/>
    /// </summary>
    /// <param name="tenantResolutionStrategy"></param>
    /// <param name="tenantStore"></param>
    public TenantService(ITenantResolutionStrategy<TKey> tenantResolutionStrategy, ITenantStore<TTenant, TKey> tenantStore)
    {
        _tenantResolutionStrategy = tenantResolutionStrategy;
        _tenantStore = tenantStore;
    }

    /// <summary>
    /// Gets the current tenant.
    /// </summary>
    /// <returns></returns>
    public async Task<TTenant> GetTenantAsync()
    {
        var tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
        return await _tenantStore.GetTenantAsync(tenantIdentifier);
    }
}
