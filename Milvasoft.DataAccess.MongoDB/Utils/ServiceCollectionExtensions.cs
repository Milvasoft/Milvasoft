using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Exceptions;
using Milvasoft.DataAccess.MongoDB.Utils.Serializers;
using Milvasoft.DataAccess.MongoDB.Utils.Settings;
using Milvasoft.Encryption.Concrete;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Milvasoft.DataAccess.MongoDB.Utils;

/// <summary>
/// Mongodb service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds related mongo services to service collection..
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddMilvaMongoHelper(this IServiceCollection services, Action<IMongoDbSettings> options)
    {
        if (options == null)
            throw new MilvaDeveloperException("Please provide mongodb settings.");

        var config = new MongoDbSettings();

        options.Invoke(config);

        if (string.IsNullOrWhiteSpace(config.DatabaseName))
            throw new MilvaDeveloperException("Please provide DatabaseName. Empty DatabaseName is not allowed.");

        if (config.MongoClientSettings == null && string.IsNullOrWhiteSpace(config.ConnectionString))
            throw new MilvaDeveloperException("Please provide ConnectionString or MongoClientSettings.");

        NoSqlRelationHelper.DbName = config.DatabaseName;

        MongoClient mongoClient;

        if (config.MongoClientSettings != null)
        {
            mongoClient = new MongoClient(config.MongoClientSettings);
        }
        else mongoClient = new MongoClient(config.ConnectionString);

        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton(mongoClient.GetDatabase(config.DatabaseName));
        services.AddSingleton<IMongoDbSettings>(config);

        if (!string.IsNullOrWhiteSpace(config.EncryptionKey))
        {
            var encryptionProvider = new MilvaEncryptionProvider(config.EncryptionKey);

            var serializer = new EncryptedStringSerializer(encryptionProvider);

            services.AddSingleton<IEncryptedStringSerializer>();

            BsonSerializer.RegisterSerializer(serializer);
        }

        if (config.AddTenantIdSupport)
        {
            BsonSerializer.RegisterSerializer(new TenantIdSerializer());
            BsonSerializer.RegisterSerializer(new NullableTenantIdSerializer());
        }

        return services;
    }
}
