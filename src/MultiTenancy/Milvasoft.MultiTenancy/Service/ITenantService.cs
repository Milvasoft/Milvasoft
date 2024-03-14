using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.MultiTenancy.Service;

/// <summary>
/// Tenant access service.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ITenantService<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Gets the current tenant.
    /// </summary>
    /// <returns></returns>
    Task<TTenant> GetTenantAsync();

}
