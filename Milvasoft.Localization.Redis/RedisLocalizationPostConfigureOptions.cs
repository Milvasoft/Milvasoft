using StackExchange.Redis;

namespace Milvasoft.Localization.Redis;

/// <summary>
/// Redis localization post configure options.
/// </summary>
public class RedisLocalizationPostConfigureOptions
{
    /// <summary>
    /// Redis configurations options can be post configured.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; }

    /// <summary>
    /// Formatted key creator delegate.
    /// </summary>
    public Func<string, string, string> KeyFormatDelegate { get; set; }
}
