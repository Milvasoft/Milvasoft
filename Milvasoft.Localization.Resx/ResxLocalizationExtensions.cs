using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.Localization.Builder;
using LocalizationOptions = Milvasoft.Localization.Builder.LocalizationOptions;

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
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder builder, Action<ResxLocalizationOptions> localizationOptions)
    {
        if (builder.Services.Any(s => s.ServiceType == typeof(ILocalizationManager)))
            throw new MilvaDeveloperException("A ILocalizationManager already registered!");

        var config = new ResxLocalizationOptions();

        localizationOptions?.Invoke(config);

        if (config.UseInMemoryCache)
        {
            if (!builder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                builder.Services.AddMemoryCache();

            builder.Services.AddSingleton<ILocalizationMemoryCache, LocalizationMemoryCache>();
        }

        config.KeyFormatDelegate ??= (string key) => string.Format(config.KeyFormat, key);

        if (!builder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)) && !string.IsNullOrWhiteSpace(config.ResourcesPath))
            builder.Services.AddLocalization(options => options.ResourcesPath = config?.ResourcesPath ?? options.ResourcesPath);

        builder.Services.AddSingleton<ILocalizationOptions>(config);
        builder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(ResxLocalizationManager<TResource>), config.ManagerLifetime));
        builder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return builder;
    }

    /// <summary>
    /// Registers <see cref="ResxLocalizationManager{TResource}"/> as <see cref="ILocalizationManager"/>.
    /// Adds <see cref="LocalizationOptions"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="configurationManager"></param>
    /// <param name="keyFormatDelegate">Post configure property.</param>
    /// <returns></returns>
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithResxManager<TResource>(localizationOptions: null);

        var section = builder.ConfigurationManager.GetSection(ResxLocalizationOptions.SectionName);

        builder.Services.AddOptions<ResxLocalizationOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<ResxLocalizationOptions>();

        builder.WithResxManager<TResource>(localizationOptions: (opt) =>
        {
            opt.ManagerLifetime = options.ManagerLifetime;
            opt.MemoryCacheEntryOptions = options.MemoryCacheEntryOptions;
            opt.UseInMemoryCache = options.UseInMemoryCache;
            opt.KeyFormat = options.KeyFormat;
        });

        return builder;
    }

    /// <summary>
    /// Post configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static LocalizationBuilder PostConfigureResxLocalizationOptions(this LocalizationBuilder builder, Action<ResxLocalizationPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<ResxLocalizationOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new ResxLocalizationPostConfigureOptions();

        postConfigureAction?.Invoke(config);

        builder.Services.UpdateSingletonInstance<ILocalizationOptions, ResxLocalizationOptions>(opt =>
        {
            opt.ResourcesPath = config.ResourcesPath ?? opt.ResourcesPath;
            opt.ResourcesFolderPath = config.ResourcesFolderPath ?? opt.ResourcesFolderPath;
            opt.KeyFormatDelegate = config.KeyFormatDelegate ?? opt.KeyFormatDelegate;
        });

        builder.Services.PostConfigure<ResxLocalizationOptions>(opt =>
        {
            opt.ResourcesPath = config.ResourcesPath ?? opt.ResourcesPath;
            opt.ResourcesFolderPath = config.ResourcesFolderPath ?? opt.ResourcesFolderPath;
            opt.KeyFormatDelegate = config.KeyFormatDelegate ?? opt.KeyFormatDelegate;
        });

        if (!builder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)) && !string.IsNullOrWhiteSpace(config.ResourcesPath))
            builder.Services.AddLocalization(options => options.ResourcesPath = config?.ResourcesPath ?? options.ResourcesPath);

        return builder;
    }
}
