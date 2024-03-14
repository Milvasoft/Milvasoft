using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.FileOperations.Builder;

/// <summary>
/// Configure caching services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="FileOperationsBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public sealed class FileOperationsBuilder(IServiceCollection services, IConfigurationManager configurationManager = null)
{
    /// <summary>
    /// Service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Configuration manager. 
    /// </summary>
    public IConfigurationManager ConfigurationManager { get; } = configurationManager;
}
