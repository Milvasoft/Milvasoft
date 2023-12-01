using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;

namespace Milvasoft.Localization.Builder;

/// <summary>
/// Milva localization options.
/// </summary>
public class LocalizationOptions : ILocalizationOptions
{
    /// <inheritdoc/>
    public ServiceLifetime ManagerLifetime { get; set; } = ServiceLifetime.Transient;

    /// <inheritdoc/>
    public string KeyFormat { get; set; }

    /// <inheritdoc/>
    public Func<string, string> KeyFormatDelegate { get; set; }

    /// <inheritdoc/>
    public bool UseInMemoryCache { get; set; } = true;

    /// <inheritdoc/>
    public MemoryCacheEntryOptions MemoryCacheEntryOptions { get; set; }
}
