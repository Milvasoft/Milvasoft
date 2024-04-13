using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class BaseEntity<TKey> : EntityBase, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";

    /// <summary>
    /// Gets the value of Id property.
    /// </summary>
    /// <returns></returns>
    public override object GetUniqueIdentifier() => Id;
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class EntityBase<TKey> : EntityBase, IEntityBase<TKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";

    /// <summary>
    /// Gets the value of Id property.
    /// </summary>
    /// <returns></returns>
    public override object GetUniqueIdentifier() => Id;
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class EntityBase : IMilvaEntity
{
    /// <summary>
    /// Initializes new instance.
    /// </summary>
    protected EntityBase()
    {
    }

    /// <summary>
    /// Gets the value of Id property.
    /// </summary>
    /// <returns></returns>
    public abstract object GetUniqueIdentifier();
}

