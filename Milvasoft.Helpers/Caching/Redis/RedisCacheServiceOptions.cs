using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Helpers.Caching.Redis
{
    /// <summary>
    /// The options relevant to a set of redis connections
    /// </summary>
    public class RedisCacheServiceOptions
    {
        /// <summary>
        /// Specifies the lifetime of a <see cref="IMilvaCacheService"/>.
        /// </summary>
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

        /// <summary>
        /// Determines object creation option.
        /// </summary>
        public bool ConnectWhenCreatingNewInstance { get; set; } = true;

        /// <summary>
        /// Redis configurations.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; }

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
}
