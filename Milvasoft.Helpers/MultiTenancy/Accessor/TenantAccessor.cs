using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Extensions;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Accessor
{
    /// <summary>
    /// Tenant accessor for easy access.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class TenantAccessor<TTenant, TKey> : ITenantAccessor<TTenant, TKey>
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates new instance of <see cref="TenantAccessor{TTenant, TKey}"/>.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public TenantAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Accessed tenant from <see cref="HttpContext"/>.
        /// </summary>
        public TTenant Tenant => _httpContextAccessor.HttpContext.GetTenant<TTenant, TKey>();
    }
}
