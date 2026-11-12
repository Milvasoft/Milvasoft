using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Accessor;
using Milvasoft.MultiTenancy.Context;
using Milvasoft.MultiTenancy.Options;
using Milvasoft.MultiTenancy.ResolutionStrategy;
using Milvasoft.MultiTenancy.Service;
using Milvasoft.MultiTenancy.Store;

namespace Milvasoft.MultiTenancy.Builder;

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
    /// Registers <see cref="TenantService{TTenant, TKey}"/> to <see cref="ITenantService{TTenant, TKey}"/> as <see cref="ServiceLifetime.Transient"/>,
    /// the scoped <see cref="ITenantContext{TTenant, TKey}"/> (also as <see cref="ITenantContext"/>) and default <see cref="TenantResolutionOptions"/>.
    /// </summary>
    /// <param name="services"></param>
    public TenantBuilder(IServiceCollection services)
    {
        services.AddTransient<ITenantAccessor<TTenant, TKey>, TenantAccessor<TTenant, TKey>>();
        services.AddTransient<ITenantService<TTenant, TKey>, TenantService<TTenant, TKey>>();
        services.TryAddScoped<TenantContext<TTenant, TKey>>();
        services.TryAddScoped<ITenantContext<TTenant, TKey>>(sp => sp.GetRequiredService<TenantContext<TTenant, TKey>>());
        services.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext<TTenant, TKey>>());
        services.TryAddSingleton(new TenantResolutionOptions());
        _services = services;
    }

    /// <summary>
    /// Configures the tenant middleware (claim binding, optional tenant requests, rejection response).
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public TenantBuilder<TTenant, TKey> WithResolutionOptions(Action<TenantResolutionOptions> configure)
    {
        var options = new TenantResolutionOptions();

        configure?.Invoke(options);

        _services.RemoveAll<TenantResolutionOptions>();
        _services.AddSingleton(options);

        return this;
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
        _services.AddHttpContextAccessor();
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
