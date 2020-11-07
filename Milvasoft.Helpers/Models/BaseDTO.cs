using System;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// <para> Base dto for all of deletable dtos. All deletable dtos have this values.</para>
    /// </summary>
    public class BaseDTO<TKey>
    {
        /// <summary>
        /// <para> Unique key. Represents primary key.</para>    
        /// </summary>    
        public TKey Id { get; set; }

        /// <summary> 
        /// <para> Date added to database for dto.</para>
        /// </summary>
        public DateTime InsertedDate { get; set; }

        /// <summary> 
        /// <para> Update date in database for dto.</para>
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

    }
}
