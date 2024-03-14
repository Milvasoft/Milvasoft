using MongoDB.Driver;

namespace Milvasoft.DataAccess.MongoDB.Utils.Settings;

/// <summary>
/// Mongo connection settings.
/// </summary>
public class MongoDbSettings : IMongoDbSettings
{
    /// <summary>
    /// Database name of to be connected database.
    /// </summary>
    public string DatabaseName { get; set; }

    /// <summary>
    /// Connection string of to be connected database. You can provide this or <see cref="MongoClientSettings"/>.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 128 bit encryption key for client side encryption. If you don't provide this, the required services for encryption will not be added.
    /// </summary>
    public string EncryptionKey { get; set; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    public bool UseUtcForDateTimes { get; set; }

    /// <summary>
    /// Adds TenantId support.
    /// </summary>
    public bool AddTenantIdSupport { get; set; }

    /// <summary>
    /// Mongo client settings. You can provide this or <see cref="ConnectionString"/>.
    /// </summary>
    public MongoClientSettings MongoClientSettings { get; set; }
}
