using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.ResolutionStrategy;
using Milvasoft.MultiTenancy.Store;

namespace Milvasoft.MultiTenancy.Service;

/// <summary>
/// Tenant access service.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <remarks>
/// Creates new instances of <see cref="TenantService{TTenant, TKey}"/>
/// </remarks>
/// <param name="tenantResolutionStrategy"></param>
/// <param name="tenantStore"></param>
public class TenantService<TTenant, TKey>(ITenantResolutionStrategy<TKey> tenantResolutionStrategy, ITenantStore<TTenant, TKey> tenantStore) : ITenantService<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    private readonly ITenantResolutionStrategy<TKey> _tenantResolutionStrategy = tenantResolutionStrategy;
    private readonly ITenantStore<TTenant, TKey> _tenantStore = tenantStore;

    /// <summary>
    /// Gets the tenant of the resolved identifier from the store. Returns null when no identifier is sent or the tenant is unknown.
    /// It does not check the user's tenant claim; prefer ITenantContext which holds the tenant validated by the tenant middleware.
    /// </summary>
    /// <returns></returns>
    public async Task<TTenant> GetTenantAsync()
    {
        var tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();

        if (Context.TenantIdentifierHelper.IsEmpty(tenantIdentifier))
            return null;

        var tenant = await _tenantStore.GetTenantAsync(tenantIdentifier);

        return tenant != null && Context.TenantIdentifierHelper.AreEqual(tenant.Id, tenantIdentifier) ? tenant : null;
    }
}
