using Milvasoft.Core.Utils;
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
}

/// <summary>
/// Base dto for all of dtos.
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

    /// <summary>
    /// Returns a collection of PropertyInfo objects that represent properties of the DTO that implements <see cref="IUpdateProperty"/>.
    /// </summary>
    /// <returns>A collection of PropertyInfo objects representing the updatable properties of the DTO.</returns>
    public virtual IEnumerable<PropertyInfo> GetUpdatableProperties() => GetDtoProperties().Where(prop => prop.PropertyType.IsGenericType
                                                                                                && (prop.PropertyType
                                                                                                       .GetGenericTypeDefinition()
                                                                                                       .IsAssignableTo(typeof(IUpdateProperty))
                                                                                                    || (Nullable.GetUnderlyingType(prop.PropertyType)?
                                                                                                                .GetGenericTypeDefinition()?
                                                                                                                .IsAssignableTo(typeof(IUpdateProperty)) ?? false)));
#pragma warning restore CA1822 // Mark members as static
}
