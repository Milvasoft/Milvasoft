using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Localization;

/// <summary>
/// Milva localization options.
/// </summary>
public class LocalizationOptions : ILocalizationOptions
{
    /// <summary>
    /// Localization manager service lifetime.
    /// </summary>
    public ServiceLifetime ManagerLifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// _ML_ : Milva Localizer
    /// {0}: app name or resource name (etc. milvawebsite or SharedResource)
    /// {1}: culture name
    /// {2}: key
    /// 
    /// full key sample :  _ML_mws_en-US_Success
    /// </summary>
    public string KeyFormat { get; set; }

    /// <summary>
    /// Fortmatted key creator delegate.
    /// </summary>
    public Func<string,string> KeyFormatDelegate { get; set; }
}
