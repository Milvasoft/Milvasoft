using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Localization;

/// <summary>
/// Configure localization services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="LocalizationBuilder"/>.
/// </remarks>
/// <param name="services"></param>
public class LocalizationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// You can register your own LocalizationManager to service collection with lifetime.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public LocalizationBuilder WithManager<TManager>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TManager : class, ILocalizationManager
    {
        Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(TManager), lifetime));

        Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return this;
    }
}
