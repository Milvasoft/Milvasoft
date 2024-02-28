using Microsoft.Extensions.Options;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Accessor;

namespace Milvasoft.MultiTenancy.Options;

/// <summary>
/// Create a new options instance with configuration applied
/// </summary>
/// <typeparam name="TOptions"></typeparam>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <remarks>
/// Initializes new instance of <see cref="TenantOptionsFactory{TOptions, TTenant, TKey}"/>
/// </remarks>
/// <param name="setups"></param>
/// <param name="postConfigures"></param>
/// <param name="tenantConfig"></param>
/// <param name="tenantAccessor"></param>
internal class TenantOptionsFactory<TOptions, TTenant, TKey>(IEnumerable<IConfigureOptions<TOptions>> setups,
                            IEnumerable<IPostConfigureOptions<TOptions>> postConfigures,
                            Action<TOptions, TTenant> tenantConfig,
                            ITenantAccessor<TTenant, TKey> tenantAccessor) : IOptionsFactory<TOptions>
    where TOptions : class, new()
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{

    private readonly IEnumerable<IConfigureOptions<TOptions>> _setups = setups;
    private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures = postConfigures;
    private readonly Action<TOptions, TTenant> _tenantConfig = tenantConfig;
    private readonly ITenantAccessor<TTenant, TKey> _tenantAccessor = tenantAccessor;

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
