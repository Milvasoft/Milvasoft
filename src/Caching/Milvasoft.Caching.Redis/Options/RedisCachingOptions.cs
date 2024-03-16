using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Caching.Redis.Options;

/// <summary>
/// The options relevant to a set of redis connections
/// </summary>
public class RedisCachingOptions : ICacheOptions<RedisCachingOptions>
{
    private string _connectionString;

    /// <summary>
    /// Configuration section path in configuration file.
    /// </summary>
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Caching:Redis";

    /// <summary>
    /// Accessor lifetime
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Redis connection string.
    /// </summary>
    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            _connectionString = value;
            ConfigurationOptions ??= new();
            ConfigurationOptions.EndPoints.Add(_connectionString);
        }
    }

    /// <summary>
    /// Redis configurations.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; set; } = new();

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    public bool UseUtcForExpirationDates { get; set; }

    /// <summary>
    /// Initializes new instance of <see cref="RedisCachingOptions"/>.
    /// </summary>
    public RedisCachingOptions()
    {
    }

    /// <summary>
    /// Initializes new instance of <see cref="RedisCachingOptions"/>.
    /// <paramref name="connectionString"/> will be added in <see cref="ConfigurationOptions.EndPoints"/>.
    /// </summary>
    /// <param name="connectionString"></param>
    public RedisCachingOptions(string connectionString)
    {
        _connectionString = connectionString;
    }
}
