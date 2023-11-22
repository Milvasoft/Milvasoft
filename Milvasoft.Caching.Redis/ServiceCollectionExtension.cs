using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Redis.Options;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// <see cref="IServiceCollection"/> extension for adding <see cref="IRedisAccessor"/>.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Adds <see cref="IRedisAccessor"/> to <see cref="IServiceCollection"/> singleton by default.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddMilvaRedisCaching(this IServiceCollection services, RedisCachingOptions options)
    {
        services.AddSingleton(options);

        //Configure other services up here
        var multiplexer = ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions).Result;

        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<IRedisCachingOptions>(options);

        return options.Lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<IRedisAccessor, RedisAccessor>(),
            ServiceLifetime.Scoped => services.AddScoped<IRedisAccessor, RedisAccessor>(),
            ServiceLifetime.Transient => services.AddTransient<IRedisAccessor, RedisAccessor>(),
            _ => services,
        };
    }
}
