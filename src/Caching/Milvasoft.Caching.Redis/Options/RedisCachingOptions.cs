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
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Caching:Redis";

    /// <summary>
    /// Accessor lifetime
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Redis connection string in StackExchange.Redis format. e.g. "localhost:6379,password=secret,ssl=false".
    /// </summary>
    /// <remarks>
    /// Setting this property replaces <see cref="ConfigurationOptions"/> with the options parsed from the connection string
    /// (all endpoints and settings in it are honored). Configure further settings on <see cref="ConfigurationOptions"/> after setting this property.
    /// </remarks>
    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            _connectionString = value;

            if (!string.IsNullOrWhiteSpace(value))
                ConfigurationOptions = ConfigurationOptions.Parse(value);
        }
    }

    /// <summary>
    /// Redis configurations.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; set; } = new();

    /// <summary>
    /// Initializes new instance of <see cref="RedisCachingOptions"/>.
    /// </summary>
    public RedisCachingOptions()
    {
    }

    /// <summary>
    /// Initializes new instance of <see cref="RedisCachingOptions"/>.
    /// <see cref="ConfigurationOptions"/> is parsed from <paramref name="connectionString"/>.
    /// </summary>
    /// <param name="connectionString"></param>
    public RedisCachingOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }
}
