using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions.Cache;

namespace Milvasoft.Caching.InMemory;

public class InMemoryCacheOptions : ICacheOptions<InMemoryCacheOptions>
{
    public ServiceLifetime AccessorLifetime { get; set; } = ServiceLifetime.Singleton;

    public MemoryCacheOptions MemoryCacheOptions { get; set; }
}
