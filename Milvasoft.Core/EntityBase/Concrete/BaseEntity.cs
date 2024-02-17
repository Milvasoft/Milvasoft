﻿using Milvasoft.Core.EntityBase.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Milvasoft.Core.EntityBase.Concrete;

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
        _entityType = this.GetType();
        _propertyInfos = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Updates entity matching properties with <paramref name="dto"/>'s not null properties.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="dto"></param>
    public void AssignUpdatedProperties<TDto>(TDto dto) where TDto : DtoBase
    {
        if (this == null || dto == null)
            return;

        foreach (var dtoProp in dto.GetDtoProperties())
        {
            var matchingEntityProp = _propertyInfos.FirstOrDefault(i => i.Name == dtoProp.Name);

            if (matchingEntityProp == null)
                continue;

            var dtoValue = dtoProp.GetValue(dto);

            if (dtoValue != null)
                matchingEntityProp.SetValue(this, dtoValue);
        }
    }
}

