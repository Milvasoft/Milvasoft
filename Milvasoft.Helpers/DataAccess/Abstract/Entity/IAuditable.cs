using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Determines entity is auditable with modifier and modification date.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAuditable<TKey> : ICreationAuditable<TKey>, IHasModifier<TKey> where TKey : struct, IEquatable<TKey>
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
    public interface IAuditable<TUser, TKey> : IAuditable<TKey>, ICreationAuditable<TUser, TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
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
    public interface IAuditableWithCustomUser<TUser, TKey> : IAuditable<TKey>, ICreationAuditableWithCustomUser<TUser, TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
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
