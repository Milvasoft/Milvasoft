using System;
using System.Threading.Tasks;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Abstraction for tenant resolution strategy.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ITenantResolutionStrategy<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Get the tenant identifier
    /// </summary>
    /// <returns></returns>
    Task<TKey> GetTenantIdentifierAsync();
}
