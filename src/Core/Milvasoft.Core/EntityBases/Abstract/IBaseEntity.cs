namespace Milvasoft.Core.EntityBases.Abstract;

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
    public new TKey Id { get; set; }
}

/// <summary>
/// Dummy interface given for constraint
/// </summary>
public interface IMilvaEntity
{
#pragma warning disable CA2011 // Avoid infinite recursion
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public virtual object Id
    {
        get => Id;
        set => Id = value;
    }
#pragma warning restore CA2011 // Avoid infinite recursion
}
