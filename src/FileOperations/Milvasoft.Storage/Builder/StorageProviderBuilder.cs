using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Models;
using Milvasoft.Storage.Providers.LocalFile;

namespace Milvasoft.Storage.Builder;

/// <summary>
/// Configure storage provider services. 
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="StorageProviderBuilder"/>.
/// </remarks>
public sealed class StorageProviderBuilder : IMilvaBuilder
{
    /// <summary>
    /// Service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Configuration manager.
    /// </summary>
    public IConfigurationManager ConfigurationManager { get; }

    /// <summary>
    /// Configuration manager.
    /// </summary>
    public StorageProviderOptions StorageProviderOptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageProviderBuilder"/> class.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <exception cref="MilvaDeveloperException"></exception>
    public StorageProviderBuilder(IServiceCollection services, IConfigurationManager configurationManager = null)
    {
        Services = services;
        ConfigurationManager = configurationManager;
        StorageProviderOptions = configurationManager?.GetSection(StorageProviderOptions.SectionName).Get<StorageProviderOptions>() ?? throw new MilvaDeveloperException("StorageProviderOptionsConfigurationMissing");

        Services.AddSingleton(StorageProviderOptions);
    }

    /// <summary>
    /// Adds local file services to service collection.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public StorageProviderBuilder WithLocalFileProvider()
    {
        if (StorageProviderOptions.LocalFile is null)
            throw new MilvaDeveloperException("LocalFileConfigurationMissing");

        Services.AddSingleton(StorageProviderOptions.LocalFile);
        Services.AddScoped<IStorageProvider, LocalFileProvider>();
        Services.AddScoped<ILocalFileProvider, LocalFileProvider>();

        return this;
    }
}
