namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IBaseEntity<TKey> : IEntityBase<TKey> where TKey : struct, IEquatable<TKey>;

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
public interface IBaseEntity : IEntityBase<string>;

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IEntityBase<TKey> : IMilvaEntity
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public new TKey Id { get; set; }
}

/// <summary>
/// Dummy interface given for constraint
/// </summary>
public interface IMilvaEntity
{
    /// <summary>
    /// Unique identifier for this entity as object. It is mainly a representation for expression/sql translations (EF Core binds it to the mapped Id property by name).
    /// When evaluated in memory it returns <see cref="GetUniqueIdentifier"/>. Use the strongly typed Id property to set the identifier.
    /// </summary>
    public object Id => GetUniqueIdentifier();

    /// <summary>
    /// Gets the value of Id property.
    /// </summary>
    /// <returns></returns>
    public abstract object GetUniqueIdentifier();
}
