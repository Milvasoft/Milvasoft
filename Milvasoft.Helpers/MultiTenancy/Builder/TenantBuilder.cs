using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Helpers.MultiTenancy.Accessor;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.ResolutionStrategy;
using Milvasoft.Helpers.MultiTenancy.Service;
using Milvasoft.Helpers.MultiTenancy.Store;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Builder
{
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

        ///// <summary>
        ///// Registers tenant specific options.
        ///// </summary>
        ///// <typeparam name="TOptions">Type of options we are apply configuration to</typeparam>
        ///// <param name="tenantConfig">Action to configure options for a tenant</param>
        ///// <returns></returns>
        //public TenantBuilder<TTenant, TKey> WithPerTenantOptions<TOptions>(Action<TOptions, TTenant> tenantConfig)
        //    where TOptions : class, new()
        //{
        //    //Register the multi-tenant cache
        //    _services.AddSingleton<IOptionsMonitorCache<TOptions>>(a => ActivatorUtilities.CreateInstance<TenantOptionsCache<TOptions, TTenant, TKey>>(a));

        //    //Register the multi-tenant options factory
        //    _services.AddTransient<IOptionsFactory<TOptions>>(a => ActivatorUtilities.CreateInstance<TenantOptionsFactory<TOptions, TTenant, TKey>>(a, tenantConfig));

        //    //Register IOptionsSnapshot support
        //    _services.AddScoped<IOptionsSnapshot<TOptions>>(a => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(a));

        //    //Register IOptions support
        //    _services.AddSingleton<IOptions<TOptions>>(a => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(a));

        //    return this;
        //}
    }
}
