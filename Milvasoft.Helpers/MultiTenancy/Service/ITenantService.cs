using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Service
{
    /// <summary>
    /// Tenant access service.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface ITenantService<TTenant, TKey>
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the current tenant.
        /// </summary>
        /// <returns></returns>
        Task<TTenant> GetTenantAsync();

    }
}
