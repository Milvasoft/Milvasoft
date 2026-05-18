using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Milvasoft.Components.Rest.MilvaResponse;

/// <summary>
/// Cursor-based paginated response.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CursorListResponse<T> : Response<List<T>>
{
    /// <summary>
    /// Cursor to pass in the next request to fetch the next page. Null if there is no next page.
    /// </summary>
    [JsonPropertyOrder(4)]
    public string NextCursor { get; set; }

    /// <summary>
    /// Cursor to pass in the next request to fetch the previous page. Null if there is no previous page.
    /// </summary>
    [JsonPropertyOrder(5)]
    public string PrevCursor { get; set; }

    /// <summary>
    /// Indicates whether there are more items beyond the current page.
    /// </summary>
    [JsonPropertyOrder(6)]
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Indicates whether there are items before the current page.
    /// </summary>
    [JsonPropertyOrder(7)]
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Total count of data matching the filter, if provided via <c>PreCalculatedTotalCount</c>.
    /// </summary>
    [JsonPropertyOrder(9)]
    public int? TotalDataCount { get; set; }

    /// <summary>
    /// Aggregation results.
    /// </summary>
    [JsonPropertyOrder(8)]
    public List<AggregationResult> AggregationResults { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="CursorListResponse{T}"/>.
    /// </summary>
    public CursorListResponse() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with <paramref name="message"/>.
    /// </summary>
    public CursorListResponse(string message) : base()
    {
        Messages = [new() { Message = message }];
    }

    /// <summary>
    /// Initializes a new instance with <paramref name="data"/> and <paramref name="message"/>.
    /// </summary>
    public CursorListResponse(List<T> data, string message) : base(data, message)
    {
    }

    #region Success

    /// <summary>
    /// Returns an empty successful <see cref="CursorListResponse{T}"/>.
    /// </summary>
    public static new CursorListResponse<T> Success() => new(LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    /// <summary>
    /// Returns a successful <see cref="CursorListResponse{T}"/> with <paramref name="data"/>.
    /// </summary>
    public static new CursorListResponse<T> Success(List<T> data) => new(data, LocalizerKeys.Successful)
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
    };

    #endregion
}
