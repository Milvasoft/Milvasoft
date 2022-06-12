namespace Milvasoft.DataAccess.MongoDB.Utils;

/// <summary>
/// Represents entity's collection name in MongoDB.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets collection name.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Creates new instance of <see cref="BsonCollectionAttribute"/>.
    /// </summary>
    /// <param name="collectionName"></param>
    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}
