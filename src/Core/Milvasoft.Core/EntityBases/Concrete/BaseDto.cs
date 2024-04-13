using Milvasoft.Types.Structs;
using System.Reflection;

namespace Milvasoft.Core.EntityBases.Concrete;

/// <summary>
/// Base dto for all of dtos.
/// </summary>
public abstract class BaseDto<TKey> : DtoBase, IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Unique identifier for this dto.
    /// </summary>
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseDto{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
    public override object GetUniqueIdentifier() => Id;
}

/// <summary>
/// Base dto for all of dtos.
/// </summary>
public abstract class DtoBase<TKey> : DtoBase, IEntityBase<TKey>
{
    /// <summary>
    /// Unique identifier for this dto.
    /// </summary>
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="DtoBase{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
    public override object GetUniqueIdentifier() => Id;
}

/// <summary>
/// Base dto for all of dtos.
/// </summary>
public abstract class DtoBase
{
    /// <summary>
    /// Initializes new instance.
    /// </summary>
    protected DtoBase()
    {
    }

    public abstract object GetUniqueIdentifier();

    /// <summary>
    /// Returns a collection of PropertyInfo objects that represent properties of the DTO that implements <see cref="IUpdateProperty"/>.
    /// </summary>
    /// <returns>A collection of PropertyInfo objects representing the updatable properties of the DTO.</returns>
    public virtual IEnumerable<PropertyInfo> GetUpdatableProperties() => GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                                                  .Where(prop => prop.PropertyType.IsGenericType
                                                                                                 && (prop.PropertyType.CanAssignableTo(typeof(IUpdateProperty))
                                                                                                     || (Nullable.GetUnderlyingType(prop.PropertyType)?
                                                                                                                 .CanAssignableTo(typeof(IUpdateProperty)) ?? false)));
}
