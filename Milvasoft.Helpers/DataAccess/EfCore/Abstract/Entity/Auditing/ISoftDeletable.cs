using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has deletion date.
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Deletion date of entity.
        /// </summary>
        DateTime? DeletionDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDeleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
