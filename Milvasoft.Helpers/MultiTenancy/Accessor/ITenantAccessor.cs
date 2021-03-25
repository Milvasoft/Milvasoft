using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Accessor
{
    /// <summary>
    /// Tenant accessor for easy access.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface ITenantAccessor<TTenant,TKey>
        where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Accessed tenant from <see cref="HttpContext"/>
        /// </summary>
        TTenant Tenant { get; }
    }
}
