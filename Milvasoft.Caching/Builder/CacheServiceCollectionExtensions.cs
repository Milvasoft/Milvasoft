using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Caching.Builder;

/// <summary>
/// Provides registration of caching services.
/// </summary>
public static class CacheServiceCollectionExtensions
{
    /// <summary>
    /// Add services the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static CacheBuilder AddMilvaCaching(this IServiceCollection services)
        => new(services);
}
