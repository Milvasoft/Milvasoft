using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Extensions;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Accessor;

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

    /// <summary>
    /// Context accessor.
    /// </summary>
    public IHttpContextAccessor HttpContextAccessor { get; }

    /// <summary>
    /// Accessed tenant from <see cref="HttpContext"/>.
    /// </summary>
    public TTenant Tenant => HttpContextAccessor.HttpContext?.GetTenant<TTenant, TKey>();

    /// <summary>
    /// Creates new instance of <see cref="TenantAccessor{TTenant, TKey}"/>.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="serviceProvider"></param>
    public TenantAccessor(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        HttpContextAccessor = httpContextAccessor;
        ServiceProvider = serviceProvider;
    }
}
