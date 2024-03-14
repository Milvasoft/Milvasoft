using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Extensions;

namespace Milvasoft.Caching.InMemory.Options;

public class InMemoryCacheOptions : ICacheOptions<InMemoryCacheOptions>
{
    public static string SectionName { get; } = $"{MilvaOptionsExtensions.ParentSectionName}:Caching:InMemory";

    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    public MemoryCacheOptions MemoryCacheOptions { get; set; }
}
