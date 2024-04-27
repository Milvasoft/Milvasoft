using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Localization.Builder;

/// <summary>
/// Milva localization options.
/// </summary>
public class LocalizationOptions : ILocalizationOptions
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Localization";

    /// <inheritdoc/>
    public ServiceLifetime ManagerLifetime { get; set; } = ServiceLifetime.Transient;

    /// <inheritdoc/>
    public string KeyFormat { get; set; }

    /// <inheritdoc/>
    public Func<string, string> KeyFormatMethod { get; set; }

    /// <inheritdoc/>
    public bool UseInMemoryCache { get; set; } = true;

    /// <inheritdoc/>
    public MemoryCacheEntryOptions MemoryCacheEntryOptions { get; set; }
}
