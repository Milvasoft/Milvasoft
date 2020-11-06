using System.Collections.Generic;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// <para> Stores pagination information. </para>
    /// </summary>
    public class PaginationDTO<TEntity>
    {
        /// <summary>
        /// <para> Items in page. </para>
        /// </summary>
        public List<TEntity> DTOList { get; set; }
        /// <summary>
        /// <para> Page count information. </para>
        /// </summary>
        public int PageCount { get; set; }
    }
}
