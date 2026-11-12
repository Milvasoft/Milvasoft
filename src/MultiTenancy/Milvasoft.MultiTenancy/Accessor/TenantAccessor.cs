using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Context;
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
    /// The validated tenant of the current scope (<see cref="ITenantContext{TTenant, TKey}"/>), falling back to <see cref="HttpContext.Items"/>.
    /// </summary>
    public TTenant Tenant => ServiceProvider?.GetService<ITenantContext<TTenant, TKey>>()?.Tenant
                             ?? HttpContextAccessor?.HttpContext?.GetTenant<TTenant, TKey>();
}
