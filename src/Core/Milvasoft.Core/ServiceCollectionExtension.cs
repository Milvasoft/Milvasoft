using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core;

/// <summary>
/// <see cref="IServiceCollection"/> extension for adding milva core components.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// It configures the Lazy implementation to work with the dependency injection container.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMilvaLazy(this IServiceCollection services) => services.AddTransient(typeof(Lazy<>), typeof(MilvaLazy<>));
}
