using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Localization.Builder;

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
    public static LocalizationBuilder AddMilvaLocalization(this IServiceCollection services, IConfigurationManager configurationManager = null)
        => new(services, configurationManager);
}
