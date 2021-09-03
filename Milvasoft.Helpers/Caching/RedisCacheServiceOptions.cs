using StackExchange.Redis;

namespace Milvasoft.Helpers.Caching
{
    /// <summary>
    /// The options relevant to a set of redis connections
    /// </summary>
    public class RedisCacheServiceOptions
    {
        /// <summary>
        /// Redis configurations.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; }

        /// <summary>
        /// Redis configurations.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Initializes new instance of <see cref="RedisCacheServiceOptions"/>.
        /// <paramref name="connectionString"/> will be added in <see cref="ConfigurationOptions.EndPoints"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        public RedisCacheServiceOptions(string connectionString)
        {
            ConnectionString = connectionString;
            ConfigurationOptions = new ConfigurationOptions();
            ConfigurationOptions.EndPoints.Add(connectionString);
        }

    }
}
