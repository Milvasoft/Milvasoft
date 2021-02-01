using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete.Entity
{
    /// <summary>
    /// Determines entity is fully auditable soft deletable entity.
    /// </summary>
    public abstract class FullAuditableEntity<TKey> : AuditableEntity<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Deletion date of entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Deleter of entity.
        /// </summary>er
        public virtual TKey? DeleterUserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDeleted.
        /// </summary>
        public virtual bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Implements <see cref="IFullAuditable{TUser, TKey}"/> to be a base class for full-audited entities.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class FullAuditableEntity<TUser, TKey> : AuditableEntity<TUser, TKey>, IFullAuditable<TUser, TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        public virtual TKey? DeleterUserId { get; set; }

        /// <summary>
        /// Reference to the deleter user of this entity.
        /// </summary>
        [ForeignKey("DeleterUserId")]
        public virtual TUser DeleterUser { get; set; }

        /// <summary>
        /// Is this entity Deleted?
        /// </summary>
        public virtual bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Implements <see cref="IFullAuditableWithCustomUser{TUser, TKey}"/> to be a base class for full-audited entities.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class FullAuditableEntityWithCustomUser<TUser, TKey> : AuditableEntityWithCustomUser<TUser, TKey>, IFullAuditableWithCustomUser<TUser, TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        public virtual TKey? DeleterUserId { get; set; }

        /// <summary>
        /// Reference to the deleter user of this entity.
        /// </summary>
        [ForeignKey("DeleterUserId")]
        public virtual TUser DeleterUser { get; set; }

        /// <summary>
        /// Is this entity Deleted?
        /// </summary>
        public virtual bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Determines entity is fully auditable without user.
    /// </summary>
    public abstract class FullAuditableEntityWithoutUser<TKey> : AuditableEntityWithoutUser<TKey>, IFullAuditableWithoutUser<TKey>
    {
        /// <summary>
        /// Deletion date of entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDeleted.
        /// </summary>
        public virtual bool IsDeleted { get; set; }
    }

}
