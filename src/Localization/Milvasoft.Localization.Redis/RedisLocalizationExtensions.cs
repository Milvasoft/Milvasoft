using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using Milvasoft.Localization.Builder;

namespace Milvasoft.Localization.Redis;

/// <summary>
/// Provides registration of redis localization services. 
/// </summary>
public static class RedisLocalizationExtensions
{
    /// <summary>
    /// Registers <see cref="RedisLocalizationManager"/> as <see cref="ILocalizationManager"/>.
    /// 
    /// <remarks> You must register <see cref="IRedisAccessor"/> for this type of use. </remarks>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="localizationOptions"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithRedisManager(this LocalizationBuilder builder, Action<RedisLocalizationOptions> localizationOptions)
    {
        if (builder.Services.Any(s => s.ServiceType == typeof(ILocalizationManager)))
            throw new MilvaDeveloperException("A ILocalizationManager already registered!");

        var config = new RedisLocalizationOptions();

        localizationOptions?.Invoke(config);

        if (config.UseInMemoryCache)
        {
            if (!builder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                builder.Services.AddMemoryCache();

            builder.Services.AddSingleton<ILocalizationMemoryCache, LocalizationMemoryCache>();
        }

        config.KeyFormatMethod ??= (string key, string cultureName) => string.Format(config.KeyFormat, cultureName, key);

        if (!builder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<RedisCachingOptions>)) && config.RedisOptions != null)
            builder.Services.AddMilvaCaching().WithRedisAccessor(config.RedisOptions);

        builder.Services.AddSingleton<ILocalizationOptions>(config);
        builder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(RedisLocalizationManager), config.ManagerLifetime));
        builder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return builder;
    }

    /// <summary>
    /// Registers <see cref="RedisLocalizationManager"/> as <see cref="ILocalizationManager"/>.
    /// Adds <see cref="LocalizationOptions"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithRedisManager(this LocalizationBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithRedisManager(localizationOptions: null);

        var section = builder.ConfigurationManager.GetSection(RedisLocalizationOptions.SectionName);

        builder.Services.AddOptions<RedisLocalizationOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<RedisLocalizationOptions>();

        builder.WithRedisManager(localizationOptions: (opt) =>
        {
            opt.ManagerLifetime = options.ManagerLifetime;
            opt.RedisOptions = options.RedisOptions;
            opt.MemoryCacheEntryOptions = options.MemoryCacheEntryOptions;
            opt.UseInMemoryCache = options.UseInMemoryCache;
            opt.KeyFormatMethod = options.KeyFormatMethod;
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
    public static LocalizationBuilder PostConfigureRedisLocalizationOptions(this LocalizationBuilder builder, Action<RedisLocalizationPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<RedisLocalizationOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new RedisLocalizationPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<ILocalizationOptions, RedisLocalizationOptions>(opt =>
        {
            opt.RedisOptions.ConfigurationOptions = config.ConfigurationOptions ?? opt.RedisOptions.ConfigurationOptions;
            opt.KeyFormatMethod = config.KeyFormatMethod ?? opt.KeyFormatMethod;
        });

        builder.Services.PostConfigure<RedisLocalizationOptions>(opt =>
        {
            opt.RedisOptions.ConfigurationOptions = config.ConfigurationOptions ?? opt.RedisOptions.ConfigurationOptions;
            opt.KeyFormatMethod = config.KeyFormatMethod ?? opt.KeyFormatMethod;
        });

        if (!builder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<RedisCachingOptions>)))
        {
            var options = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(ILocalizationOptions))?.ImplementationInstance;

            if (options != null)
                builder.Services.AddMilvaCaching().WithRedisAccessor(((RedisLocalizationOptions)options).RedisOptions);
        }

        return builder;
    }
}
