using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Caching.Builder;

/// <summary>
/// Configure caching services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="CacheBuilder"/>.
/// </remarks>
/// <param name="services"></param>
public sealed class CacheBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
