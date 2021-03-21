using Autofac;
using Microsoft.Extensions.Options;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Options;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Extensions
{
    /// <summary>
    /// Provides registration of tenant options.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers tenant specific options.
        /// </summary>
        /// <typeparam name="TOptions">Type of options we are apply configuration to</typeparam>
        /// <typeparam name="TTenant">Type of options we are apply configuration to</typeparam>
        /// <typeparam name="TKey">Type of options we are apply configuration to</typeparam>
        /// <param name="builder"></param>
        /// <param name="tenantConfig">Action to configure options for a tenant</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterTenantOptions<TOptions, TTenant, TKey>(this ContainerBuilder builder, Action<TOptions, TTenant> tenantConfig)
            where TOptions : class, new()
           where TTenant : class, IMilvaTenantBase<TKey>
            where TKey : IEquatable<TKey>
        {
            builder.RegisterType<TenantOptionsCache<TOptions, TTenant, TKey>>()
                .As<IOptionsMonitorCache<TOptions>>()
                .SingleInstance();

            builder.RegisterType<TenantOptionsFactory<TOptions, TTenant, TKey>>()
                .As<IOptionsFactory<TOptions>>()
                .WithParameter(new TypedParameter(typeof(Action<TOptions, TTenant>), tenantConfig))
                .SingleInstance();


            builder.RegisterType<TenantOptions<TOptions>>()
                .As<IOptionsSnapshot<TOptions>>()
                .SingleInstance();

            builder.RegisterType<TenantOptions<TOptions>>()
                .As<IOptions<TOptions>>()
                .SingleInstance();

            return builder;
        }
    }


}
