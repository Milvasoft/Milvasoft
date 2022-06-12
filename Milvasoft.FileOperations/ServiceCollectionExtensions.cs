using Microsoft.Extensions.DependencyInjection;
using Milvasoft.FileOperations.Abstract;
using Milvasoft.FileOperations.Concrete;
using System;

namespace Milvasoft.FileOperations;

/// <summary>
/// Provides json file operations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds json file operations services to service collection. Adds <see cref="IJsonOperationsConfig"/> as singleton to services too.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddJsonOperations(this IServiceCollection services,
                                                       ServiceLifetime lifetime = ServiceLifetime.Singleton,
                                                       Action<IJsonOperationsConfig> options = null)
    {
        var config = new JsonOperationsConfig();

        options.Invoke(config);

        services.AddSingleton<IJsonOperationsConfig>(config);

        return lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<IJsonOperations, JsonOperations>(),
            ServiceLifetime.Scoped => services.AddScoped<IJsonOperationsConfig>(),
            ServiceLifetime.Transient => services.AddTransient<IJsonOperationsConfig>(),
            _ => services,
        };
    }
}
