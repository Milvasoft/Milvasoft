using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete.Entity
{
    /// <summary>
    /// Determines entity's creation is auditable.
    /// </summary>
    public abstract class CreationAuditableEntity<TKey> : BaseEntity<TKey>, ICreationAuditable<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Creation date of entity.
        /// </summary>
        public virtual DateTime CreationDate { get; set; }

        /// <summary>
        /// Creator of entity.
        /// </summary>
        public virtual TKey? CreatorUserId { get; set; }

    }

    /// <summary>
    /// This class can be used to simplify implementing <see cref="ICreationAuditable{TUser, TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class CreationAuditableEntity<TUser, TKey> : CreationAuditableEntity<TKey>, ICreationAuditable<TUser, TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Reference to the creator user of this entity.
        /// </summary>
        [ForeignKey("CreatorUserId")]
        public virtual TUser CreatorUser { get; set; }
    }

    /// <summary>
    /// This class can be used to simplify implementing <see cref="ICreationAuditableWithCustomUser{TUser, TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class CreationAuditableEntityEntityWithCustomUser<TUser, TKey> : CreationAuditableEntity<TKey>, ICreationAuditableWithCustomUser<TUser, TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
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
    public abstract class CreationAuditableEntityWithoutUser<TKey> : EntityBase<TKey>, ICreationAuditableWithoutUser<TKey>
    {
        /// <summary>
        /// Creation date of entity.
        /// </summary>
        public virtual DateTime CreationDate { get; set; }
    }
}
