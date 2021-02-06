using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;

namespace Milvasoft.Helpers.Identity.Concrete
{
    /// <summary>
    /// Milva user.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class MilvaRole<TKey> : IdentityRole<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Creation date of user.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Creator of user.
        /// </summary>
        public Guid? CreatorUserId { get; set; }

        /// <summary>
        /// Last modificationd date of user.
        /// </summary>
        public DateTime? LastModificationDate { get; set; }

        /// <summary>
        /// Last Modifier User of user.
        /// </summary>
        public Guid? LastModifierUserId { get; set; }

        /// <summary>
        /// Deletion date of user.
        /// </summary>
        public DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Deleter of user.
        /// </summary>
        public Guid? DeleterUserId { get; set; }

        /// <summary>
        /// Gets or sets wheter user is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
