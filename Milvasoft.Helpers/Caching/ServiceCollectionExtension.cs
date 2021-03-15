using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Helpers.Caching
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension for adding <see cref="IRedisCacheService"/>.
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Adds <see cref="IRedisCacheService"/> to <see cref="IServiceCollection"/> by <see cref="RedisCacheServiceOptions.Lifetime"/>.
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
                    return services.AddSingleton<IRedisCacheService, RedisCacheService>();
                case ServiceLifetime.Scoped:
                    return services.AddScoped<IRedisCacheService, RedisCacheService>();
                case ServiceLifetime.Transient:
                    return services.AddTransient<IRedisCacheService, RedisCacheService>();
            }

            return services;
        }

    }

   

}
