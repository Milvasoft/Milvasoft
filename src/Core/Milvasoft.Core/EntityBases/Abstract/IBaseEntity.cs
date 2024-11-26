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
    /// Unique identifier for this entity. Do not use this property when access Id. This is a representation for sql translations etc.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2011:Avoid infinite recursion", Justification = "<Pending>")]
    public virtual object Id
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Gets the value of Id property.
    /// </summary>
    /// <returns></returns>
    public abstract object GetUniqueIdentifier();
}
