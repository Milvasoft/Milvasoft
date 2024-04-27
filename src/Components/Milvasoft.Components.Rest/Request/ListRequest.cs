namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// List request specs.
/// </summary>
public record ListRequest : IListRequest
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
    /// Filter criterias.
    /// </summary>
    public FilterRequest Filtering { get; set; }

    /// <summary>
    /// Sort criterias.
    /// </summary>
    public SortRequest Sorting { get; set; }

    /// <summary>
    /// Aggregation criterias.
    /// </summary>
    public AggregationRequest Aggregation { get; set; }

    /// <summary>
    /// Calculates the total number of pages based on the requested page number and row count, and compares it with the estimated count of pages.
    /// </summary>
    /// <param name="totalDataCount">The total number of data items.</param>
    /// <returns>The estimated count of pages.</returns>
    public int CalculatePageCountAndCompareWithRequested(int? totalDataCount)
    {
        // Check if the page number or row count is null
        if (PageNumber == null || RowCount == null)
            return 0;

        // Check if the page number is less than or equal to 0
        if (PageNumber.HasValue && PageNumber <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        // Check if the row count is less than or equal to 0
        if (RowCount.HasValue && RowCount <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);

        // Calculate the actual page count
        var actualPageCount = Convert.ToDouble(totalDataCount) / Convert.ToDouble(RowCount);

        // Estimate the count of pages
        var estimatedCountOfPages = Convert.ToInt32(Math.Ceiling(actualPageCount));

        // Check if the estimated count of pages is not 0 and the requested page number is greater than the estimated count of pages
        if (estimatedCountOfPages != 0 && PageNumber > estimatedCountOfPages)
            throw new MilvaUserFriendlyException(MilvaException.WrongPaginationParams, estimatedCountOfPages);

        return estimatedCountOfPages;
    }
}

/// <summary>
/// List request abstraction.
/// </summary>
public interface IListRequest
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
    /// Filter criterias.
    /// </summary>
    public FilterRequest Filtering { get; set; }

    /// <summary>
    /// Sort criterias.
    /// </summary>
    public SortRequest Sorting { get; set; }

    /// <summary>
    /// Aggregation criterias.
    /// </summary>
    public AggregationRequest Aggregation { get; set; }
}