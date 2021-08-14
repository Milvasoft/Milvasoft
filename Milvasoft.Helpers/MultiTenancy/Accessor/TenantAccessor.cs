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
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Application service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates new instance of <see cref="TenantAccessor{TTenant, TKey}"/>.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="serviceProvider"></param>
        public TenantAccessor(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Accessed tenant from <see cref="HttpContext"/>.
        /// </summary>
        public TTenant Tenant => _httpContextAccessor.HttpContext.GetTenant<TTenant, TKey>();
    }
}
