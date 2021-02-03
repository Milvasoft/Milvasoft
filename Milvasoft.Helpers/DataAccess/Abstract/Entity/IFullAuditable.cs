using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Determines entity is fully auditable with user information.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IFullAuditable<TKey> : IAuditable<TKey>, IHasDeletionDate where TKey : struct, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Determines entity is fully auditable with user information.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public interface IFullAuditable<TUserKey, TKey> : IAuditable<TKey>, IHasDeletionDate, IHasDeleter<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IFullAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public interface IFullAuditable<TUser, TUserKey, TKey> : IFullAuditable<TUserKey, TKey>, IAuditable<TUser, TUserKey, TKey>
        where TUser : IdentityUser<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IFullAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public interface IFullAuditableWithCustomUser<TUser, TUserKey, TKey> : IFullAuditable<TUserKey, TKey>, IAuditableWithCustomUser<TUser, TUserKey, TKey>
        where TUser : IBaseEntity<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
    }

    /// <summary>
    /// Determines entity is fully auditable.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IFullAuditableWithoutUser<TKey> : IAuditableWithoutUser<TKey>, IHasDeletionDate
    {
    }
}
