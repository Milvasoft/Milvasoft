using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Extensions;

namespace Milvasoft.Caching.InMemory;

public class InMemoryCacheOptions : ICacheOptions<InMemoryCacheOptions>
{
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Localization";

    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    public MemoryCacheOptions MemoryCacheOptions { get; set; }
}
