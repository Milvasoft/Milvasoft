using Milvasoft.Core.Exceptions;
using System.Collections.Generic;

namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// List request specs.
/// </summary>
public class ListRequest : FilterableAndSortable
{
    /// <summary>
    /// Requested page number
    /// </summary>
    /// <example>1</example>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Rows per page
    /// </summary>
    /// <example>10</example>
    public int? RowCount { get; set; }

    /// <summary>
    /// Checks pagination parameters are reasonable.
    /// </summary>
    /// <param name="totalDataCount"></param>
    /// <returns></returns>
    public int CalculatePageCountAndCompareWithRequested(int? totalDataCount)
    {
        if (PageNumber == null || RowCount == null)
            return 0;

        if (PageNumber.HasValue && PageNumber <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        if (RowCount.HasValue && RowCount <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);

        var actualPageCount = Convert.ToDouble(totalDataCount) / Convert.ToDouble(RowCount);

        var estimatedCountOfPages = Convert.ToInt32(Math.Ceiling(actualPageCount));

        if (estimatedCountOfPages != 0 && PageNumber > estimatedCountOfPages)
            throw new MilvaUserFriendlyException(MilvaException.WrongPaginationParams, estimatedCountOfPages);

        return estimatedCountOfPages;
    }
}
