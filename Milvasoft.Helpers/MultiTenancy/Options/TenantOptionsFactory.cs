using Microsoft.Extensions.Options;
using Milvasoft.Helpers.MultiTenancy.Accessor;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Collections.Generic;

namespace Milvasoft.Helpers.MultiTenancy.Options
{
    /// <summary>
    /// Create a new options instance with configuration applied
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal class TenantOptionsFactory<TOptions, TTenant, TKey> : IOptionsFactory<TOptions>
        where TOptions : class, new()
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : struct, IEquatable<TKey>
    {

        private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures;
        private readonly Action<TOptions, TTenant> _tenantConfig;
        private readonly ITenantAccessor<TTenant, TKey> _tenantAccessor;

        /// <summary>
        /// Initializes new instance of <see cref="TenantOptionsFactory{TOptions, TTenant, TKey}"/>
        /// </summary>
        /// <param name="setups"></param>
        /// <param name="postConfigures"></param>
        /// <param name="tenantConfig"></param>
        /// <param name="tenantAccessor"></param>
        public TenantOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> setups,
                                    IEnumerable<IPostConfigureOptions<TOptions>> postConfigures,
                                    Action<TOptions, TTenant> tenantConfig,
                                    ITenantAccessor<TTenant, TKey> tenantAccessor)
        {
            _setups = setups;
            _postConfigures = postConfigures;
            _tenantAccessor = tenantAccessor;
            _tenantConfig = tenantConfig;
        }

        /// <summary>
        /// Creates a new options instance.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TOptions Create(string name)
        {
            var options = new TOptions();

            //Apply options setup configuration
            foreach (var setup in _setups)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else
                {
                    setup.Configure(options);
                }
            }

            //Apply tenant specifc configuration (to both named and non-named options)
            if (_tenantAccessor.Tenant != null)
                _tenantConfig(options, _tenantAccessor.Tenant);

            //Apply post configuration
            foreach (var postConfig in _postConfigures)
            {
                postConfig.PostConfigure(name, options);
            }

            return options;
        }
    }
}