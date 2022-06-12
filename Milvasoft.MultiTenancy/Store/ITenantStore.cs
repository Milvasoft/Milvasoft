using Milvasoft.Core.EntityBase.MultiTenancy;
using System;
using System.Threading.Tasks;

namespace Milvasoft.MultiTenancy.Store;

/// <summary>
/// Abstraction for tenant storage. 
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ITenantStore<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Gets tenant according to <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    Task<TTenant> GetTenantAsync(TKey identifier);
}
