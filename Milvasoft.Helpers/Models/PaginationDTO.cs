using System.Collections.Generic;

namespace Milvasoft.Helpers.Models;

/// <summary>
/// Stores pagination information.
/// </summary>
public class PaginationDTO<TEntity>
{
    /// <summary>
    /// Items in page. 
    /// </summary>
    public List<TEntity> DTOList { get; set; }

    /// <summary>
    /// Page count information.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Total data count of all pages.
    /// </summary>
    public int TotalDataCount { get; set; }
}
