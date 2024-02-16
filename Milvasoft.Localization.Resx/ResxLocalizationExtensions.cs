using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
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
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder localizationBuilder, Action<ResxLocalizationOptions> localizationOptions = null)
    {
        if (localizationBuilder.Services.Any(s => s.ServiceType == typeof(ILocalizationManager)))
            throw new MilvaDeveloperException("A ILocalizationManager already registered!");

        var config = new ResxLocalizationOptions();

        localizationOptions?.Invoke(config);

        if (config.UseInMemoryCache)
        {
            if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                localizationBuilder.Services.AddMemoryCache();

            localizationBuilder.Services.AddSingleton<ILocalizationMemoryCache, LocalizationMemoryCache>();
        }

        config.KeyFormatDelegate ??= (string key) => string.Format(config.KeyFormat, key);

        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IStringLocalizerFactory)))
            localizationBuilder.Services.AddLocalization(options => options.ResourcesPath = config?.ResourcesPath ?? options.ResourcesPath);

        localizationBuilder.Services.AddSingleton<ILocalizationOptions>(config);
        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(ResxLocalizationManager<TResource>), config.ManagerLifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }

    /// <summary>
    /// Registers <see cref="ResxLocalizationManager{TResource}"/> as <see cref="ILocalizationManager"/>.
    /// Adds <see cref="LocalizationOptions"/> as <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="configurationManager"></param>
    /// <param name="keyFormatDelegate">Post configure property.</param>
    /// <returns></returns>
    public static LocalizationBuilder WithResxManager<TResource>(this LocalizationBuilder builder, string resourceFolderPath = null, string resourcesPath = null, Func<string, string> keyFormatDelegate = null)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithResxManager<TResource>(localizationOptions: null);

        var section = builder.ConfigurationManager.GetSection(ResxLocalizationOptions.SectionName);

        builder.Services.AddOptions<ILocalizationOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        builder.Services.AddOptions<ResxLocalizationOptions>()
                .Bind(section)
                .ValidateDataAnnotations();


        builder.Services.PostConfigure<ResxLocalizationOptions>(opt =>
        {
            opt.KeyFormatDelegate = keyFormatDelegate ?? opt.KeyFormatDelegate;
            opt.ResourcesFolderPath = resourceFolderPath ?? opt.ResourcesFolderPath;
            opt.ResourcesPath = resourcesPath ?? opt.ResourcesPath;
        });

        builder.Services.PostConfigure<ILocalizationOptions>(opt =>
        {
            opt.KeyFormatDelegate = keyFormatDelegate ?? opt.KeyFormatDelegate;
        });

        builder.Services.PostConfigure<LocalizationOptions>(opt =>
        {
            opt.KeyFormatDelegate = keyFormatDelegate ?? opt.KeyFormatDelegate;
        });

        var options = section.Get<ResxLocalizationOptions>();

        options.KeyFormatDelegate = keyFormatDelegate ?? options.KeyFormatDelegate;

        builder.WithResxManager<TResource>(localizationOptions: (opt) =>
        {
            opt.ManagerLifetime = options.ManagerLifetime;
            opt.MemoryCacheEntryOptions = options.MemoryCacheEntryOptions;
            opt.UseInMemoryCache = options.UseInMemoryCache;
            opt.KeyFormatDelegate = options.KeyFormatDelegate;
            opt.KeyFormat = options.KeyFormat;
            opt.ResourcesPath = options.ResourcesPath;
            opt.ResourcesFolderPath = options.ResourcesFolderPath;
        });

        return builder;
    }
}
