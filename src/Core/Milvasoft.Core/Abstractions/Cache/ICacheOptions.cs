using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions.Cache;

/// <summary>
/// Makes IOptions cache aware.
/// </summary>
/// <typeparam name="TCacheOptions"></typeparam>
#pragma warning disable S2326 // Unused type parameters should be removed
public interface ICacheOptions<TCacheOptions> : IMilvaOptions where TCacheOptions : class
#pragma warning restore S2326 // Unused type parameters should be removed
{
    /// <summary>
    /// Caching accessor service lifetime.
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; }
}
