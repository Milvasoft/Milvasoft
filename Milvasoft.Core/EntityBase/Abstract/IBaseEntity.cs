namespace Milvasoft.Core.EntityBase.Abstract;

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IBaseEntity<TKey> : IEntityBase<TKey> where TKey : struct, IEquatable<TKey>
{
}

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
public interface IBaseEntity : IEntityBase<string>
{
}

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IEntityBase<TKey> : IMilvaEntity
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public TKey Id { get; set; }
}

/// <summary>
/// Dummy interface given for constraint
/// </summary>
public interface IMilvaEntity
{

}
