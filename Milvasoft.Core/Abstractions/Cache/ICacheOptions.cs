using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions.Cache;

public interface ICacheOptions<TCacheOptions> : IMilvaOptions where TCacheOptions : class
{
    /// <summary>
    /// Caching accessor service lifetime.
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; }
}
