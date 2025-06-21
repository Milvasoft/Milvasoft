using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Storage.Builder;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> to add storage provider services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static StorageProviderBuilder AddMilvaStorageProvider(this IServiceCollection services, IConfigurationManager configurationManager = null)
        => new(services, configurationManager);
}
