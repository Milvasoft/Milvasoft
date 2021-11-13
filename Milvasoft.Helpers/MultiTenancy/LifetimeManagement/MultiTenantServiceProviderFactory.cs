using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;

namespace Milvasoft.Helpers.MultiTenancy.LifetimeManagement;

/// <summary>
/// Registration for custom service provider.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class MultiTenantServiceProviderFactory<TTenant, TKey> : IServiceProviderFactory<ContainerBuilder>
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{

    /// <summary>
    /// Gets or sets tenant services configuration.
    /// </summary>
    public Action<TTenant, ContainerBuilder> _tenantServicesConfiguration;

    /// <summary>
    /// Initializes new instance of <see cref="MultiTenantServiceProviderFactory{TTenant, TKey}"/>
    /// </summary>
    /// <param name="tenantServicesConfiguration"></param>
    public MultiTenantServiceProviderFactory(Action<TTenant, ContainerBuilder> tenantServicesConfiguration)
    {
        _tenantServicesConfiguration = tenantServicesConfiguration;
    }

    /// <summary>
    /// Create a builder populated with global services.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public ContainerBuilder CreateBuilder(IServiceCollection services)
    {
        var builder = new ContainerBuilder();

        builder.Populate(services);

        return builder;
    }

    /// <summary>
    /// Create our serivce provider.
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <returns></returns>
    public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
    {
        MultiTenantContainer<TTenant, TKey> container = null;

        Func<MultiTenantContainer<TTenant, TKey>> containerAccessor = () =>
        {
            return container;
        };

        containerBuilder.RegisterInstance(containerAccessor).SingleInstance();

        container = new MultiTenantContainer<TTenant, TKey>(containerBuilder.Build(), _tenantServicesConfiguration);

        return new AutofacServiceProvider(containerAccessor());
    }
}
