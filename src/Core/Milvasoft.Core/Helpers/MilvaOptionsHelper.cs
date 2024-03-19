using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Milvasoft.Core.Helpers;

/// <summary>
/// Milvasoft library components options extensions.
/// </summary>
public static partial class CommonHelper
{
    /// <summary>
    /// Adds <see cref="LocalizationOptions"/> as <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static IMilvaBuilder ConfigureOptions<TOptions>(this IMilvaBuilder builder, IConfigurationManager configurationManager) where TOptions : class, IMilvaOptions
    {
        var section = configurationManager.GetSection(TOptions.SectionName);

        builder.Services.AddOptions<TOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        return builder;
    }
}
