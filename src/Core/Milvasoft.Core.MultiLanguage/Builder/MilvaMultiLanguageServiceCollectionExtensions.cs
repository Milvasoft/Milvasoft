using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.MultiLanguage.Builder;

/// <summary>
/// Implements the multi language features.
/// </summary>
public static class MilvaMultiLanguageServiceCollectionExtensions
{
    /// <summary>
    /// Adds the milva multi language features to service collection.
    /// </summary>
    /// <param name="services">The services available in the application.</param>
    /// <returns>An <see cref="MilvaMultiLanguageBuilder"/> for creating and configuring the identity system.</returns>
    public static MilvaMultiLanguageBuilder AddMilvaMultiLanguage(this IServiceCollection services)
        => new(services);
}
