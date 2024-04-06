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
public sealed class CacheBuilder(IServiceCollection services, IConfigurationManager configurationManager = null)
{
    public IServiceCollection Services { get; } = services;
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
        Services.AddScoped<ICacheAccessor<TAccessor>, TAccessor>();

        return this;
    }
}
