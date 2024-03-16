using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Localization.Builder;

/// <summary>
/// Configure localization services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="LocalizationBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public sealed class LocalizationBuilder(IServiceCollection services, IConfigurationManager configurationManager = null) : IMilvaBuilder
{
    public IServiceCollection Services { get; } = services;
    public IConfigurationManager ConfigurationManager { get; } = configurationManager;

    /// <summary>
    /// You can register your own LocalizationManager to service collection with lifetime.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public LocalizationBuilder WithManager<TManager>(Action<ILocalizationOptions> localizationOptions = null) where TManager : class, ILocalizationManager
    {
        var config = new LocalizationOptions();

        localizationOptions?.Invoke(config);

        if (config.UseInMemoryCache)
        {
            if (!Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                Services.AddMemoryCache();

            Services.AddSingleton<ILocalizationMemoryCache, LocalizationMemoryCache>();
        }

        Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(TManager), config.ManagerLifetime));

        Services.AddSingleton(config);
        Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return this;
    }
}
