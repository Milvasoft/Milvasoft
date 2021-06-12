namespace Milvasoft.Helpers.DataAccess.MongoDB.Utils
{
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
        /// Connection string of to be connected database.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
