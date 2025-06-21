using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Exceptions;
using Milvasoft.Storage.Abstract;
using Milvasoft.Storage.Builder;

namespace Milvasoft.Storage.S3;

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
    public static StorageProviderBuilder WithAwsS3Provider(this StorageProviderBuilder builder)
    {
        var s3Configuration = builder.StorageProviderOptions.AwsS3 ?? throw new MilvaDeveloperException("S3ConfigurationMissing");

        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(s3Configuration.AccessKey, s3Configuration.SecretKey),
            Region = RegionEndpoint.GetBySystemName(s3Configuration.Region)
        };

        builder.Services.AddSingleton(builder.StorageProviderOptions.AwsS3);
        builder.Services.AddAWSService<IAmazonS3>(awsOptions);
        builder.Services.AddScoped<IStorageProvider, S3Provider>();
        builder.Services.AddScoped<IS3Provider, S3Provider>();

        return builder;
    }
}
