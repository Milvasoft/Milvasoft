using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis.Options;

/// <summary>
/// The options relevant to a set of redis connections
/// </summary>
public class RedisCachingOptions : IRedisCachingOptions
{
    /// <summary>
    /// Specifies the lifetime of a <see cref="IRedisAccessor"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Redis configurations.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    public bool UseUtcForExpirationDates { get; set; }

    /// <summary>
    /// Initializes new instance of <see cref="RedisCachingOptions"/>.
    /// <paramref name="connectionString"/> will be added in <see cref="ConfigurationOptions.EndPoints"/>.
    /// </summary>
    /// <param name="connectionString"></param>
    public RedisCachingOptions(string connectionString)
    {
        ConfigurationOptions = new ConfigurationOptions();
        ConfigurationOptions.EndPoints.Add(connectionString);
    }

}
