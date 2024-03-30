using Milvasoft.DataAccess.MongoDB.Entity.Abstract;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Milvasoft.DataAccess.MongoDB.Entity.Concrete;

/// <summary>
/// Base entity for entities. 
/// If <see cref="Environment"/>.ASPNETCORE_ENVIRONMENT variable is 'Development' you can set <see cref="CreationDate"/> prop.
/// If <see cref="Environment"/>.ASPNETCORE_ENVIRONMENT variable is 'Production' you cannot set <see cref="CreationDate"/>, this prop gets <see cref="ObjectId.CreationTime"/>.
/// </summary>
public abstract class DocumentBase : IDocumentBase
{
    private static readonly bool _developmentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    private DateTime? _creationDate;

    /// <summary>
    /// Unique key of entity.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public virtual ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    /// Last modification date of entity.
    /// </summary>
    public virtual DateTime? LastModificationDate { get; set; }

    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime? CreationDate
    {
        get => _developmentEnv ? _creationDate : Id.CreationTime;
        set
        {
            if (_developmentEnv)
                _creationDate = value;
        }
    }

    /// <summary>
    /// Gets the Id property value of entity.
    /// </summary>
    /// <returns></returns>
    public object GetId() => Id;
}
