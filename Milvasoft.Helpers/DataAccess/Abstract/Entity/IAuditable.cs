using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using Milvasoft.Helpers.Identity.Concrete;
using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Determines entity is auditable with modifier and modification date.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAuditable<TKey> : ICreationAuditable<TKey>where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        DateTime? LastModificationDate { get; set; }
    }

    /// <summary>
    /// Determines entity is auditable with modifier and modification date.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public interface IAuditable<TUserKey,TKey> : ICreationAuditable<TKey>, IHasModifier<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        DateTime? LastModificationDate { get; set; }
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Primary key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public interface IAuditable<TUser, TUserKey, TKey> : IAuditable<TUserKey,TKey>, ICreationAuditable<TUser, TUserKey, TKey>
        where TUser : IdentityUser<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Reference to the last modifier user of this entity.
        /// </summary>
        TUser LastModifierUser { get; set; }
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IAuditable{TKey}"/> interface for custom user.
    /// </summary>
    /// <typeparam name="TKey">Primary key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public interface IAuditableWithCustomUser<TUser, TUserKey, TKey> : IAuditable<TUserKey,TKey>, ICreationAuditableWithCustomUser<TUser, TUserKey, TKey>
        where TUser : IBaseEntity<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Reference to the last modifier user of this entity.
        /// </summary>
        TUser LastModifierUser { get; set; }
    }

    /// <summary>
    /// Determines entity is auditable without modifier.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAuditableWithoutUser<TKey> : ICreationAuditableWithoutUser<TKey>
    {
        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        DateTime? LastModificationDate { get; set; }
    }
}
