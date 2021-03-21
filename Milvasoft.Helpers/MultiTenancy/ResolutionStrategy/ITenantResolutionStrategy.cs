using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.ResolutionStrategy
{
    public interface ITenantResolutionStrategy<TKey> where TKey :  IEquatable<TKey>
    {
        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <returns></returns>
        Task<TKey> GetTenantIdentifierAsync();
    }
}
