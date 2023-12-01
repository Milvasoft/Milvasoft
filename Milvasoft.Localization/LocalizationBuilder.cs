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
public sealed class LocalizationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// You can register your own LocalizationManager to service collection with lifetime.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public LocalizationBuilder WithManager<TManager>(ILocalizationOptions localizationOptions = null) where TManager : class, ILocalizationManager
    {
        localizationOptions ??= new LocalizationOptions();

        Services.Add(ServiceDescriptor.Describe(typeof(ILocalizationManager), typeof(TManager), localizationOptions.ManagerLifetime));

        Services.AddSingleton(localizationOptions);
        Services.AddTransient<IMilvaLocalizer, MilvaLocalizer>();

        return this;
    }
}
