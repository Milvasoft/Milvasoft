using Milvasoft.Core.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

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

    public EntityBase()
    {
        _entityType = GetType();
        _propertyInfos = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public Type GetEntityType() => _entityType;
    public PropertyInfo[] GetDtoProperties() => _propertyInfos;
}

