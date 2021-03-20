using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.ResolutionStrategy
{
    public interface ITenantResolutionStrategy<TIdentifier> where TIdentifier :  IEquatable<TIdentifier>
    {
        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <returns></returns>
        Task<TIdentifier> GetTenantIdentifierAsync();
    }
}
