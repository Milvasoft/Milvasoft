using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Abstractions;

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
    public static LocalizationBuilder WithRedisManager(this LocalizationBuilder localizationBuilder, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IRedisCachingOptions)))
            throw new Exception("Please add required Milva redis caching services to service collection. You can use services.AddMilvaRedisCaching(). Or you can use this method overloads");

        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(RedisLocalizationManager), lifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }

    /// <summary>
    /// Registers <see cref="RedisLocalizationManager"/> as <see cref="ILocalizationManager"/>.
    /// Adds Milva redis caching services to service collection.
    /// </summary>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static LocalizationBuilder WithRedisManager(this LocalizationBuilder localizationBuilder, RedisCachingOptions redisOptions, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (!localizationBuilder.Services.Any(s => s.ServiceType == typeof(IRedisCachingOptions)))
            localizationBuilder.Services.AddMilvaRedisCaching(redisOptions);

        localizationBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(RedisLocalizationManager), lifetime));
        localizationBuilder.Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return localizationBuilder;
    }
}
