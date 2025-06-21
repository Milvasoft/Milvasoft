using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Exceptions;
using Milvasoft.Storage.Abs.AzureAbs;
using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Builder;

namespace Milvasoft.Storage.Abs;

/// <summary>
/// Extensions to add storage provider services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds aws s3 services to service collection.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static StorageProviderBuilder WithAzureAbsProvider(this StorageProviderBuilder builder)
    {
        var absConfiguration = builder.StorageProviderOptions.AzureBlob ?? throw new MilvaDeveloperException("ABSConfigurationMissing");

        var blobServiceClient = new BlobServiceClient(absConfiguration.ConnectionString);

        var containerClient = blobServiceClient.GetBlobContainerClient(absConfiguration.ContainerName);

        containerClient.CreateIfNotExists();

        //register blob service client by reading StorageAccount connection string from appsettings.json
        builder.Services.AddSingleton(x => new BlobServiceClient(absConfiguration.ConnectionString));

        builder.Services.AddSingleton(builder.StorageProviderOptions.AzureBlob);
        builder.Services.AddScoped<IStorageProvider, AbsProvider>();
        builder.Services.AddScoped<IAbsProvider, AbsProvider>();

        return builder;
    }
}
