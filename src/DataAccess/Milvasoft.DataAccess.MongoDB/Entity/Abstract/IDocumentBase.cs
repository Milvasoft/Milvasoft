using MongoDB.Bson;

namespace Milvasoft.DataAccess.MongoDB.Entity.Abstract;

/// <summary>
/// Base interface for most obk entities.
/// </summary>
public interface IDocumentBase : IAuditableWithoutUser<ObjectId>
{
}
