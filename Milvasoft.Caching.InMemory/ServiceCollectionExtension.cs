using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Core.Abstractions.Cache;

namespace Milvasoft.Caching.InMemory;

/// <summary>
/// <see cref="IServiceCollection"/> extension for adding <see cref="IRedisAccessor"/>.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers <see cref="RedisAccessor"/> as <see cref="ICacheAccessor"/>.
    /// </summary>
    /// <param name="cacheBuilder"></param>
    /// <param name="cachingOptions"></param>
    /// <returns></returns>
    public static CacheBuilder WithInMemoryAccessor(this CacheBuilder cacheBuilder, InMemoryCacheOptions cachingOptions = null)
    {
        if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
            cacheBuilder.Services.AddMemoryCache(opt =>
            {
                if (cachingOptions != null)
                {
                    opt.ExpirationScanFrequency = cachingOptions.MemoryCacheOptions.ExpirationScanFrequency;
                    opt.TrackLinkedCacheEntries = cachingOptions.MemoryCacheOptions.TrackLinkedCacheEntries;
                    opt.CompactionPercentage = cachingOptions.MemoryCacheOptions.CompactionPercentage;
                    opt.TrackStatistics = cachingOptions.MemoryCacheOptions.TrackStatistics;
                    opt.SizeLimit = cachingOptions.MemoryCacheOptions.SizeLimit;
                    opt.Clock = cachingOptions.MemoryCacheOptions.Clock;
                }
            });

        if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheAccessor<MemoryCacheAccessor>)))
        {
            if (cachingOptions != null)
            {
                if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<InMemoryCacheOptions>)))
                    cacheBuilder.Services.AddSingleton<ICacheOptions<InMemoryCacheOptions>>(cachingOptions);

                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<MemoryCacheAccessor>), typeof(MemoryCacheAccessor), cachingOptions.AccessorLifetime));
            }
            else
            {
                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<MemoryCacheAccessor>), typeof(MemoryCacheAccessor), ServiceLifetime.Singleton));
            }
        }

        return cacheBuilder;
    }

}
