using Milvasoft.Core.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Base entity for all of entities.
/// </summary>
public abstract class BaseDto<TKey> : DtoBase, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
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
public abstract class DtoBase<TKey> : DtoBase, IEntityBase<TKey>
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
public abstract class DtoBase
{
    private static Type _dtoType;
    private static PropertyInfo[] _propertyInfos;

    /// <summary>
    /// Initializes new instance.
    /// </summary>
    public DtoBase()
    {
        _dtoType = GetType();
        _propertyInfos = _dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets dto type.
    /// </summary>
    /// <returns></returns>
#pragma warning disable CA1822 // Mark members as static
    public Type GetDtoType() => _dtoType;

    /// <summary>
    /// Get dto type properties.
    /// </summary>
    /// <returns></returns>
    public PropertyInfo[] GetDtoProperties() => _propertyInfos;
#pragma warning restore CA1822 // Mark members as static
}
