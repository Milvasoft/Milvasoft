using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory.Accessor;
using Milvasoft.Caching.InMemory.Options;
using Milvasoft.Core.Abstractions.Cache;

namespace Milvasoft.Caching.InMemory;

/// <summary>
/// <see cref="IServiceCollection"/> extension for adding <see cref="IMemoryCacheAccessor"/>.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers <see cref="MemoryCacheAccessor"/> as <see cref="ICacheAccessor"/>.
    /// </summary>
    /// <param name="cacheBuilder"></param>
    /// <param name="cachingOptions"></param>
    /// <returns></returns>
    public static CacheBuilder WithInMemoryAccessor(this CacheBuilder cacheBuilder, InMemoryCacheOptions cachingOptions)
    {
        if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(IMemoryCache)))
            cacheBuilder.Services.AddMemoryCache(opt =>
            {
                var memoryCacheOptions = cachingOptions?.MemoryCacheOptions;

                if (memoryCacheOptions != null)
                {
                    opt.ExpirationScanFrequency = memoryCacheOptions.ExpirationScanFrequency;
                    opt.TrackLinkedCacheEntries = memoryCacheOptions.TrackLinkedCacheEntries;
                    opt.CompactionPercentage = memoryCacheOptions.CompactionPercentage;
                    opt.TrackStatistics = memoryCacheOptions.TrackStatistics;
                    opt.SizeLimit = memoryCacheOptions.SizeLimit;
                    opt.Clock = memoryCacheOptions.Clock;
                }
            });

        if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheAccessor<MemoryCacheAccessor>)))
        {
            if (cachingOptions != null)
            {
                if (!cacheBuilder.Services.Any(s => s.ServiceType == typeof(ICacheOptions<InMemoryCacheOptions>)))
                    cacheBuilder.Services.AddSingleton<ICacheOptions<InMemoryCacheOptions>>(cachingOptions);

                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<MemoryCacheAccessor>), typeof(MemoryCacheAccessor), cachingOptions.AccessorLifetime));
                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(IMemoryCacheAccessor), typeof(MemoryCacheAccessor), cachingOptions.AccessorLifetime));
            }
            else
            {
                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(ICacheAccessor<MemoryCacheAccessor>), typeof(MemoryCacheAccessor), ServiceLifetime.Singleton));
                cacheBuilder.Services.Add(ServiceDescriptor.Describe(typeof(IMemoryCacheAccessor), typeof(MemoryCacheAccessor), ServiceLifetime.Singleton));
            }
        }

        return cacheBuilder;
    }

    /// <summary>
    /// Registers <see cref="MemoryCacheAccessor"/> as <see cref="ICacheAccessor"/>.
    /// Adds <see cref="ICacheOptions{TOptions}"/> as <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static CacheBuilder WithInMemoryAccessor(this CacheBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithInMemoryAccessor(cachingOptions: null);

        var section = builder.ConfigurationManager.GetSection(InMemoryCacheOptions.SectionName);

        builder.Services.AddOptions<InMemoryCacheOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<InMemoryCacheOptions>();

        builder.WithInMemoryAccessor(options);

        return builder;
    }
}
