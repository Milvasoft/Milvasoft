namespace Milvasoft.DataAccess.MongoDB.Utils.Attributes;

/// <summary>
/// Represents entity's collection name in MongoDB.
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="BsonCollectionAttribute"/>.
/// </remarks>
/// <param name="collectionName"></param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute(string collectionName) : Attribute
{
    /// <summary>
    /// Gets or sets collection name.
    /// </summary>
    public string CollectionName { get; } = collectionName;
}
