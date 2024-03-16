using Microsoft.AspNetCore.Http;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Extensions;

namespace Milvasoft.MultiTenancy.Accessor;

/// <summary>
/// Tenant accessor for easy access.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <remarks>
/// Creates new instance of <see cref="TenantAccessor{TTenant, TKey}"/>.
/// </remarks>
/// <param name="httpContextAccessor"></param>
/// <param name="serviceProvider"></param>
public class TenantAccessor<TTenant, TKey>(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) : ITenantAccessor<TTenant, TKey>
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Application service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Context accessor.
    /// </summary>
    public IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;

    /// <summary>
    /// Accessed tenant from <see cref="HttpContext"/>.
    /// </summary>
    public TTenant Tenant => HttpContextAccessor.HttpContext?.GetTenant<TTenant, TKey>();
}
