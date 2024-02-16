using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Extensions;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis.Options;

/// <summary>
/// The options relevant to a set of redis connections
/// </summary>
public class RedisCachingOptions : ICacheOptions<RedisCachingOptions>
{
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Caching:Redis";

    /// <summary>
    /// Accessor lifetime
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Redis connection string.
    /// </summary>
    public string ConnectionString { get; }

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
        ConnectionString = connectionString;
        ConfigurationOptions = new ConfigurationOptions();
        ConfigurationOptions.EndPoints.Add(connectionString);
    }

}
