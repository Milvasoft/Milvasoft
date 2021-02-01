using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Determines entity's creation is auditable with user information.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ICreationAuditable<TKey> : IBaseEntity<TKey>, IHasCreator<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Creation date of entity.
        /// </summary>
        public DateTime CreationDate { get; set; }
    }

    /// <summary>
    /// Adds navigation properties to <see cref="ICreationAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public interface ICreationAuditable<TUser, TKey> : ICreationAuditable<TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
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
    public interface ICreationAuditableWithCustomUser<TUser, TKey> : ICreationAuditable<TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
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
}
