using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder localizationBuilder, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)))
            throw new Exception("Please add required Microsoft localization services to service collection. You can use services.AddLocalization(). Or you can use this method overloads");

        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(ResxLocalizationManager<TResource>), lifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }


    /// <summary>
    /// Registers <see cref="ResxLocalizationManager{TResource}"/> as <see cref="ILocalizationManager"/>.
    /// Adds Milva redis caching services to service collection.
    /// </summary>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder localizationBuilder, string resourcesPath, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)))
            localizationBuilder.Services.AddLocalization(options => options.ResourcesPath = resourcesPath);

        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(ResxLocalizationManager<TResource>), lifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }
}
