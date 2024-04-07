using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.FileOperations.Builder;

/// <summary>
/// Configure cryptography services.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="MilvaCryptographyBuilder"/>.
/// </remarks>
/// <param name="services"></param>
/// <param name="configurationManager"></param>
public sealed class MilvaCryptographyBuilder(IServiceCollection services, IConfigurationManager configurationManager = null)
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
