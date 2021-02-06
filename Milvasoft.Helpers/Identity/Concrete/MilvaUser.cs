using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;

namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Milva user.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class MilvaUser<TKey> : IdentityUser<TKey>, IFullAuditable<MilvaUser<TKey>,TKey,TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Creation date of user.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Creator of user.
        /// </summary>
        public TKey? CreatorUserId { get; set; }

        /// <summary>
        /// Navigation property of creator user.
        /// </summary>
        public virtual MilvaUser<TKey> CreatorUser { get; set; }

        /// <summary>
        /// Last modificationd date of user.
        /// </summary>
        public DateTime? LastModificationDate { get; set; }

        /// <summary>
        /// Last Modifier User of user.
        /// </summary>
        public TKey? LastModifierUserId { get; set; }

        /// <summary>
        /// Navigation property of last modifier user.
        /// </summary>
        public virtual MilvaUser<TKey> LastModifierUser { get; set; }

        /// <summary>
        /// Deletion date of user.
        /// </summary>
        public DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Deleter of user.
        /// </summary>
        public TKey? DeleterUserId { get; set; }

        /// <summary>
        /// Navigation property of deleter user.
        /// </summary>
        public virtual MilvaUser<TKey> DeleterUser { get; set; }

        /// <summary>
        /// Gets or sets wheter user is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
