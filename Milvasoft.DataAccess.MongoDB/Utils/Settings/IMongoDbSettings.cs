using MongoDB.Driver;

namespace Milvasoft.DataAccess.MongoDB.Utils.Settings;

/// <summary>
/// Interface of mongo connection settings.
/// </summary>
public interface IMongoDbSettings
{
    /// <summary>
    /// Database name of to be connected database.
    /// </summary>
    string DatabaseName { get; set; }

    /// <summary>
    /// Connection string of to be connected database. You can provide this or <see cref="MongoClientSettings"/>.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// 128 bit encryption key for client side encryption. If you don't provide this, the required services for encryption will not be added.
    /// </summary>
    string EncryptionKey { get; set; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    bool UseUtcForDateTimes { get; set; }

    /// <summary>
    /// Adds TenantId support.
    /// </summary>
    bool AddTenantIdSupport { get; set; }

    /// <summary>
    /// Mongo client settings. You can provide this or <see cref="ConnectionString"/>.
    /// </summary>
    MongoClientSettings MongoClientSettings { get; set; }
}
