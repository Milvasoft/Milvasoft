using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Localization;

/// <summary>
/// Provides registration of localization services.
/// </summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Add services the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static LocalizationBuilder AddMilvaLocalization(this IServiceCollection services)      
        => new(services);
}
