using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Caching.InMemory.Options;

public class InMemoryCacheOptions : ICacheOptions<InMemoryCacheOptions>
{
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Caching:InMemory";

    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    public MemoryCacheOptions MemoryCacheOptions { get; set; }
}
