using System;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// Base dto for all of deletable dtos. All deletable dtos have this values.
    /// </summary>
    [Obsolete("This class will removed in next versions. Please use Auditable entities in Milvasoft.Helpers.DataAccess instead.")]
    public class BaseDTO<TKey>
    {
        /// <summary>
        /// Unique key. Represents primary key.
        /// </summary>    
        public TKey Id { get; set; }

        /// <summary> 
        /// Date added to database for dto.
        /// </summary>
        public DateTime InsertedDate { get; set; }

        /// <summary> 
        /// Update date in database for dto.
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }

    }
}
