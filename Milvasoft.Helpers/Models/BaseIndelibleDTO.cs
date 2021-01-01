using System;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// Base DTO for all of indelible entities. All indelible DTOs have this values.
    /// </summary>
    public class BaseIndelibleDTO<TKey> : BaseDTO<TKey>
    {
        /// <summary>
        /// Delete date in database for entity.
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// State of entity in the database. Default value is true.
        /// </summary>
        public bool IsDeleted { get; set; } = true;
    }
}
