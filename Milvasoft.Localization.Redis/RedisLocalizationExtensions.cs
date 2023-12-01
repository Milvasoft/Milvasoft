using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Abstractions;
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
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithRedisManager(this LocalizationBuilder localizationBuilder, Action<RedisLocalizationOptions> localizationOptions = null)
    {
        var config = new RedisLocalizationOptions();

        localizationOptions?.Invoke(config);

        if (config.UseInMemoryCache)
        {
            if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                localizationBuilder.Services.AddMemoryCache();

            localizationBuilder.Services.AddSingleton<ILocalizationMemoryCache, LocalizationMemoryCache>();
        }

        config.KeyFormatDelegate ??= (string key, string cultureName) => string.Format(config.KeyFormat, cultureName, key);

        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IRedisCachingOptions)) && config.RedisOptions != null)
            localizationBuilder.Services.AddMilvaRedisCaching(config.RedisOptions);

        localizationBuilder.Services.AddSingleton<ILocalizationOptions>(config);
        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(RedisLocalizationManager), config.ManagerLifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }
}
