using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Caching.Redis;

namespace Milvasoft.Helpers.Caching
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension for adding <see cref="IMilvaCacheService"/>.
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Adds <see cref="IMilvaCacheService"/> to <see cref="IServiceCollection"/> by <see cref="RedisCacheServiceOptions.Lifetime"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddMilvaRedisCaching(this IServiceCollection services, RedisCacheServiceOptions options)
        {
            services.AddSingleton(options);

            switch (options.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return services.AddSingleton<IMilvaCacheService, RedisCacheService>();
                case ServiceLifetime.Scoped:
                    return services.AddScoped<IMilvaCacheService, RedisCacheService>();
                case ServiceLifetime.Transient:
                    return services.AddTransient<IMilvaCacheService, RedisCacheService>();
            }

            return services;
        }

    }

   

}
