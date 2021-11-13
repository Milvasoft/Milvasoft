using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Helpers.MultiTenancy.Accessor;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.ResolutionStrategy;
using Milvasoft.Helpers.MultiTenancy.Service;
using Milvasoft.Helpers.MultiTenancy.Store;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Builder;

/// <summary>
/// Configure tenant services.
/// </summary>
public class TenantBuilder<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Creates new instance of <see cref="TenantBuilder{TTenant, TKey}"/>.
    /// Registers <see cref="TenantService{TTenant, TKey}"/> to <see cref="ITenantService{TTenant, TKey}"/> as <see cref="ServiceLifetime.Transient"/>
    /// </summary>
    /// <param name="services"></param>
    public TenantBuilder(IServiceCollection services)
    {
        services.AddTransient<ITenantAccessor<TTenant, TKey>, TenantAccessor<TTenant, TKey>>();
        services.AddTransient<ITenantService<TTenant, TKey>, TenantService<TTenant, TKey>>();
        _services = services;
    }

    /// <summary>
    /// Registers the tenant resolver implementation.
    /// </summary>
    /// <typeparam name="TResolutionStrategy"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public TenantBuilder<TTenant, TKey> WithResolutionStrategy<TResolutionStrategy>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TResolutionStrategy : class, ITenantResolutionStrategy<TKey>
    {
        _services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        _services.Add(ServiceDescriptor.Describe(typeof(ITenantResolutionStrategy<TKey>), typeof(TResolutionStrategy), lifetime));
        return this;
    }

    /// <summary>
    /// Registers the tenant store implementation.
    /// </summary>
    /// <typeparam name="TStore"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public TenantBuilder<TTenant, TKey> WithStore<TStore>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TStore : class, ITenantStore<TTenant, TKey>
    {
        _services.Add(ServiceDescriptor.Describe(typeof(ITenantStore<TTenant, TKey>), typeof(TStore), lifetime));
        return this;
    }
}
