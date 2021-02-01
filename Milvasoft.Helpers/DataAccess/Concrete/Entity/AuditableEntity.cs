using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.Helpers.DataAccess.Concrete.Entity
{
    /// <summary>
    /// Determines entity's is auditable with modifier and modification date.
    /// </summary>
    public abstract class AuditableEntity<TKey> : CreationAuditableEntity<TKey>, IAuditable<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        public virtual DateTime? LastModificationDate { get; set; }

        /// <summary>
        /// Last modifier of entity.
        /// </summary>
        public virtual TKey? LastModifierUserId { get; set; }
    }

    /// <summary>
    /// This class can be used to simplify implementing <see cref="IAuditable{TUser}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class AuditableEntity<TUser, TKey> : AuditableEntity<TKey>, IAuditable<TUser, TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Reference to the creator user of this entity.
        /// </summary>
        [ForeignKey("CreatorUserId")]
        public virtual TUser CreatorUser { get; set; }

        /// <summary>
        /// Reference to the last modifier user of this entity.
        /// </summary>
        [ForeignKey("LastModifierUserId")]
        public virtual TUser LastModifierUser { get; set; }
    }

    /// <summary>
    /// This class can be used to simplify implementing <see cref="IAuditable{TUser}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key of the entity</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public abstract class AuditableEntityWithCustomUser<TUser, TKey> : AuditableEntity<TKey>, IAuditableWithCustomUser<TUser, TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Reference to the creator user of this entity.
        /// </summary>
        [ForeignKey("CreatorUserId")]
        public virtual TUser CreatorUser { get; set; }

        /// <summary>
        /// Reference to the last modifier user of this entity.
        /// </summary>
        [ForeignKey("LastModifierUserId")]
        public virtual TUser LastModifierUser { get; set; }
    }

    /// <summary>
    /// Determines entity's is auditable with modifier and modification date.
    /// </summary>
    public abstract class AuditableEntityWithoutUser<TKey> : CreationAuditableEntityWithoutUser<TKey>, IAuditableWithoutUser<TKey>
    {
        /// <summary>
        /// Last modification date of entity.
        /// </summary>
        public virtual DateTime? LastModificationDate { get; set; }
    }
}
