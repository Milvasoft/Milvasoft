using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Caching.Redis.Options;
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
            if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(IConnectionMultiplexer)) && cachingOptions.ConfigurationOptions != null)
            {
                //Configure other services up here
                var multiplexer = ConnectionMultiplexer.Connect(cachingOptions.ConfigurationOptions);

                cacheBuilder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            }

            if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<RedisCachingOptions>)))
                cacheBuilder.Services.AddSingleton<ICacheOptions<RedisCachingOptions>>(cachingOptions);

            cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<RedisAccessor>), typeof(RedisAccessor), cachingOptions.AccessorLifetime));
            cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(IRedisAccessor), typeof(RedisAccessor), cachingOptions.AccessorLifetime));
        }

        return cacheBuilder;
    }

    /// <summary>
    /// Registers <see cref="RedisAccessor"/> as <see cref="ICacheAccessor"/>.
    /// Adds <see cref="ICacheOptions{TOptions}"/> as <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static CacheBuilder WithRedisAccessor(this CacheBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithRedisAccessor(cachingOptions: null);

        var section = builder.ConfigurationManager.GetSection(RedisCachingOptions.SectionName);

        builder.Services.AddOptions<RedisCachingOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<RedisCachingOptions>();

        builder.WithRedisAccessor(options);

        return builder;
    }
}
