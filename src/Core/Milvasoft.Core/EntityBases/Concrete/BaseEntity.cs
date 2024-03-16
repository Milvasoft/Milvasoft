using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

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
}

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class EntityBase
{
    private static Type _entityType;
    private static PropertyInfo[] _propertyInfos;

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public EntityBase()
    {
        _entityType = GetType();
        _propertyInfos = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Gets entity type.
    /// </summary>
    /// <returns></returns>
    public Type GetEntityType() => _entityType;

    /// <summary>
    /// Gets entity's type properties from static collection.
    /// </summary>
    /// <returns></returns>
    public PropertyInfo[] GetEntityProperties() => _propertyInfos;
#pragma warning restore CA1822 // Mark members as static
}

