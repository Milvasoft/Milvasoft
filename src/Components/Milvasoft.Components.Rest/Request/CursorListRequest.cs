namespace Milvasoft.Components.Rest.Request;

/// <summary>
/// Cursor-based pagination list request specs.
/// </summary>
public record CursorListRequest
{
    /// <summary>
    /// Opaque cursor value representing the pagination position. If null, fetches from the beginning.
    /// </summary>
    public string Cursor { get; set; }

    /// <summary>
    /// Number of rows per page.
    /// </summary>
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
    /// An optional pre-calculated total count of the data. If provided, it will be used instead of querying the database for the count.
    /// Useful for PostgreSQL performance use cases where count queries are expensive.
    /// </summary>
    public int? PreCalculatedTotalCount { get; set; }
}
