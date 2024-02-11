using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Abstractions.Cache;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis;

/// <summary>
/// <see cref="IServiceCollection"/> extension for adding <see cref="IRedisAccessor"/>.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers <see cref="RedisAccessor"/> as <see cref="ICacheAccessor{T}"/>.
    /// </summary>
    /// <param name="cacheBuilder"></param>
    /// <param name="cachingOptions"></param>
    /// <returns></returns>
    public static CacheBuilder WithRedisAccessor(this CacheBuilder cacheBuilder, RedisCachingOptions cachingOptions)
    {
        if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheAccessor<RedisAccessor>)) && cachingOptions != null)
        {
            if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(IConnectionMultiplexer)))
            {
                //Configure other services up here
                var multiplexer = ConnectionMultiplexer.Connect(cachingOptions.ConfigurationOptions);

                cacheBuilder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            }

            if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<RedisCachingOptions>)))
                cacheBuilder.Services.AddSingleton<ICacheOptions<RedisCachingOptions>>(cachingOptions);

            cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<RedisAccessor>), typeof(RedisAccessor), cachingOptions.AccessorLifetime));
        }

        return cacheBuilder;
    }
}
