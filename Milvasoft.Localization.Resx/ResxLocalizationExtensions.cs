using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Localization.Resx;

/// <summary>
/// Provides registration of resx localization services.
/// </summary>
public static class ResxLocalizationExtensions
{
    /// <summary>
    /// Registers <see cref="ResxLocalizationManager{TResource}"/> as <see cref="ILocalizationManager"/>.
    /// Adds Microsoft localiztion services to service collection.
    /// </summary>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder localizationBuilder, Action<ResxLocalizationOptions> localizationOptions = null)
    {
        var config = new ResxLocalizationOptions();

        localizationOptions?.Invoke(config);

        config.KeyFormatDelegate ??= (string key) => string.Format(config.KeyFormat, key);

        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)) && !string.IsNullOrEmpty(config.ResourcesPath))
            localizationBuilder.Services.AddLocalization(options => options.ResourcesPath = config.ResourcesPath);

        localizationBuilder.Services.AddSingleton<ILocalizationOptions>(config);
        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(ResxLocalizationManager<TResource>), config.ManagerLifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }
}
