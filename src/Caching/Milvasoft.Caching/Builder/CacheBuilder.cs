using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Caching.Builder;

/// <summary>
/// Configure caching services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="CacheBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public sealed class CacheBuilder(IServiceCollection services, IConfigurationManager configurationManager = null)
{
    /// <summary>
    /// Services collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Configuration manager.
    /// </summary>
    public IConfigurationManager ConfigurationManager { get; } = configurationManager;

    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types.
    /// </summary>
    /// <returns></returns>
    public CacheBuilder WithAccessor<TAccessor, TCacheOptions>(TCacheOptions options)
        where TAccessor : class, ICacheAccessor<TAccessor>
        where TCacheOptions : class, ICacheOptions<TCacheOptions>
    {
        Services.AddSingleton<ICacheOptions<TCacheOptions>>(options);
        Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<TAccessor>), typeof(TAccessor), options.AccessorLifetime));

        return this;
    }
}
