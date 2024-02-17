using Microsoft.AspNetCore.Identity;
using Milvasoft.Core.EntityBases.Abstract.Auditing;

namespace Milvasoft.Core.EntityBases.Abstract;

/// <summary>
/// Determines entity's creation is auditable with user information.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditable<TKey> : IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public DateTime CreationDate { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable with user information.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TUserKey"></typeparam>
public interface ICreationAuditable<TUserKey, TKey> : ICreationAuditable<TKey>, IHasCreator<TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
}

/// <summary>
/// Adds navigation properties to <see cref="ICreationAuditable{TKey}"/> interface for user.
/// </summary>
/// <typeparam name="TKey">Key of the user</typeparam>
/// <typeparam name="TUser">Type of the user</typeparam>
/// <typeparam name="TUserKey">Type of the user</typeparam>
public interface ICreationAuditable<TUser, TUserKey, TKey> : ICreationAuditable<TUserKey, TKey>
    where TUser : IdentityUser<TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    TUser CreatorUser { get; set; }
}

/// <summary>
/// Adds navigation properties to <see cref="ICreationAuditable{TKey}"/> interface for user.
/// </summary>
/// <typeparam name="TKey">Key of the user</typeparam>
/// <typeparam name="TUser">Type of the user</typeparam>
/// <typeparam name="TUserKey">Type of the user</typeparam>
public interface ICreationAuditableWithCustomUser<TUser, TUserKey, TKey> : ICreationAuditable<TUserKey, TKey>
    where TUser : IBaseEntity<TUserKey>
    where TKey : struct, IEquatable<TKey>
    where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Reference to the creator user of this entity.
    /// </summary>
    TUser CreatorUser { get; set; }
}

/// <summary>
/// Determines entity's creation is auditable.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface ICreationAuditableWithoutUser<TKey> : IEntityBase<TKey>
{
    /// <summary>
    /// Creation date of entity.
    /// </summary>
    public DateTime CreationDate { get; set; }
}
