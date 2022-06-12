using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBase.MultiTenancy;
using Milvasoft.MultiTenancy.Builder;

namespace Milvasoft.MultiTenancy.Extensions;

/// <summary>
/// Provides registration of custom tenant services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the services (application specific tenant class)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<TTenant, TKey> AddMultiTenancy<TTenant, TKey>(this IServiceCollection services)
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : struct, IEquatable<TKey>
        => new(services);
}
