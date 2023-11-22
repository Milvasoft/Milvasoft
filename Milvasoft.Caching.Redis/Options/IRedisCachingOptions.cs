using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis.Options;

/// <summary>
/// Milva caching options.
/// </summary>
public interface IRedisCachingOptions
{
    /// <summary>
    /// Specifies the lifetime of a <see cref="IRedisAccessor"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Redis configurations.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    public bool UseUtcForExpirationDates { get; set; }
}