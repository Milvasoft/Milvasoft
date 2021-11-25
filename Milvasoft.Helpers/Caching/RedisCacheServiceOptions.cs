using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Helpers.Caching;

/// <summary>
/// The options relevant to a set of redis connections
/// </summary>
public class RedisCacheServiceOptions
{
    /// <summary>
    /// Specifies the lifetime of a <see cref="IRedisCacheService"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Redis configurations.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    public bool UseUtcForExpirationDates { get; set; }

    /// <summary>
    /// Initializes new instance of <see cref="RedisCacheServiceOptions"/>.
    /// <paramref name="connectionString"/> will be added in <see cref="ConfigurationOptions.EndPoints"/>.
    /// </summary>
    /// <param name="connectionString"></param>
    public RedisCacheServiceOptions(string connectionString)
    {
        ConfigurationOptions = new ConfigurationOptions();
        ConfigurationOptions.EndPoints.Add(connectionString);
    }

}
