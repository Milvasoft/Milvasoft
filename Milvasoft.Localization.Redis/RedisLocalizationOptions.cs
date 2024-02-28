using Milvasoft.Caching.Redis.Options;
using Milvasoft.Core.Extensions;
using Milvasoft.Localization.Builder;

namespace Milvasoft.Localization.Redis;

/// <summary>
/// Resx manager spesific options.
/// </summary>
public class RedisLocalizationOptions : LocalizationOptions
{
    public new static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Localization:Redis";

    /// <summary>
    /// _ML_ : Milva Localizer
    /// <para>{0}: culture name</para>
    /// <para>{1}: key</para>
    /// 
    /// full key sample :  _ML_en-US_Success
    /// </summary>
    public new string KeyFormat { get; set; } = "_ML_{0}_{1}";

    /// <summary>
    /// Milva redis caching options
    /// </summary>
    public RedisCachingOptions RedisOptions { get; set; } = new();

    /// <summary>
    /// Formatted key creator delegate.
    /// Default is: (string key, string cultureName) => string.Format(config.KeyFormat, cultureName, key);
    /// </summary>
    public new Func<string, string, string> KeyFormatDelegate { get; set; }
}
