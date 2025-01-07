using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions.Cache;

/// <summary>
/// Makes IOptions cache aware.
/// </summary>
/// <typeparam name="TCacheOptions"></typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "<Pending>")]
public interface ICacheOptions<TCacheOptions> : IMilvaOptions where TCacheOptions : class
{
    /// <summary>
    /// Caching accessor service lifetime.
    /// </summary>
    public ServiceLifetime AccessorLifetime { get; set; }
}
