﻿using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Milvasoft.Helpers.Caching
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension for adding <see cref="IRedisCacheService"/>.
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Adds <see cref="IRedisCacheService"/> to <see cref="IServiceCollection"/> by singleton.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddMilvaRedisCaching(this IServiceCollection services, RedisCacheServiceOptions options)
        {
            services.AddSingleton(options);

            //Configure other services up here
            var multiplexer = ConnectionMultiplexer.ConnectAsync(options.ConfigurationOptions).Result;

            services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            return services.AddSingleton<IRedisCacheService, RedisCacheService>();
        }

    }



}
