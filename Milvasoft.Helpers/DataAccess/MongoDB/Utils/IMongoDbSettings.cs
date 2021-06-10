namespace Milvasoft.Helpers.DataAccess.MongoDB.Utils
{
    public interface IMongoDbSettings
    {
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
    }
}
