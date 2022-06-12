using Microsoft.AspNetCore.Identity;
using Milvasoft.Core.EntityBase.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Core.EntityBase.Concrete;

/// <summary>
/// Determines entity's creation is auditable.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntity<TKey> : BaseEntity<TKey>, ICreationAuditable<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime CreationDate { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
/// <typeparam name="TUserKey">Type of the user</typeparam>
public abstract class CreationAuditableEntity<TUserKey, TKey> : BaseEntity<TKey>, ICreationAuditable<TUserKey, TKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime CreationDate { get; set; }

    /// <summary>
    /// Creator of entity.
    /// </summary>
    public virtual TUserKey? CreatorUserId { get; set; }
}

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAuditable{TUser,TUserKey, TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
/// <typeparam name="TUser">Type of the user</typeparam>
/// <typeparam name="TUserKey">Type of the user</typeparam>
public abstract class CreationAuditableEntity<TUser, TUserKey, TKey> : CreationAuditableEntity<TUserKey, TKey>, ICreationAuditable<TUser, TUserKey, TKey>
    where TUser : IdentityUser<TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    [ForeignKey("CreatorUserId")]
    public virtual TUser CreatorUser { get; set; }
}

/// <summary>
/// This class can be used to simplify implementing <see cref="ICreationAuditableWithCustomUser{TUser,TUserKey, TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
/// <typeparam name="TUser">Type of the user</typeparam>
/// <typeparam name="TUserKey">Type of the user</typeparam>
public abstract class CreationAuditableEntityWithCustomUser<TUser, TUserKey, TKey> : CreationAuditableEntity<TUserKey, TKey>, ICreationAuditableWithCustomUser<TUser, TUserKey, TKey>
    where TUser : IBaseEntity<TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    [ForeignKey("CreatorUserId")]
    public virtual TUser CreatorUser { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable without user.
/// </summary>
/// <typeparam name="TKey">Type of the user</typeparam>
public abstract class CreationAuditableEntityWithoutUser<TKey> : EntityBase<TKey>, ICreationAuditableWithoutUser<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public virtual DateTime CreationDate { get; set; }
}
