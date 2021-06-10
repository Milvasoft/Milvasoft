namespace Milvasoft.Helpers.DataAccess.MongoDB.Utils
{
    public class MongoDbSettings : IMongoDbSettings
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }
}
