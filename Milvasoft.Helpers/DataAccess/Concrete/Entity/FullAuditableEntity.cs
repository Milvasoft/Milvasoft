using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.Identity.Concrete;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete.Entity
{
    /// <summary>
    /// Determines entity is fully auditable soft deletable entity.
    /// </summary>
    /// <typeparam name="TKey">Type of the user</typeparam>
    public abstract class FullAuditableEntity<TKey> : AuditableEntity<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
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

    /// <summary>
    /// Determines entity is fully auditable soft deletable entity.
    /// </summary>
    /// <typeparam name="TKey">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public abstract class FullAuditableEntity<TUserKey, TKey> : AuditableEntity<TUserKey, TKey>, IFullAuditable<TUserKey, TKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Deletion date of entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Deleter of entity.
        /// </summary>er
        public virtual TUserKey? DeleterUserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDeleted.
        /// </summary>
        public virtual bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Implements <see cref="IFullAuditable{TUser,TUserKey,TKey}"/> to be a base class for full-audited entities.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public abstract class FullAuditableEntity<TUser, TUserKey, TKey> : AuditableEntity<TUser, TUserKey, TKey>, IFullAuditable<TUser, TUserKey, TKey>
        where TUser : MilvaUser<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        public virtual TUserKey? DeleterUserId { get; set; }

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
    /// Implements <see cref="IFullAuditableWithCustomUser{TUser,TUserKey, TKey}"/> to be a base class for full-audited entities.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    /// <typeparam name="TUserKey">Type of the user</typeparam>
    public abstract class FullAuditableEntityWithCustomUser<TUser, TUserKey, TKey> : AuditableEntityWithCustomUser<TUser, TUserKey, TKey>, IFullAuditableWithCustomUser<TUser, TUserKey, TKey>
        where TUser : IBaseEntity<TUserKey>
        where TKey : struct, IEquatable<TKey>
        where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        public virtual DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        public virtual TUserKey? DeleterUserId { get; set; }

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
    /// <typeparam name="TKey">Type of the user</typeparam>
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
