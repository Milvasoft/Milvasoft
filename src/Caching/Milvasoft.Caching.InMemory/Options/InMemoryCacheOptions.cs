using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Caching.InMemory.Options;

/// <summary>
/// In memory cache options. 
/// </summary>
public class InMemoryCacheOptions : ICacheOptions<InMemoryCacheOptions>
{
    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Caching:InMemory";

    /// <inheritdoc/>
    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// In memory cache specific options.
    /// </summary>
    public MemoryCacheOptions MemoryCacheOptions { get; set; }
}
