using Milvasoft.Components.Rest.Enums;

namespace Milvasoft.Components.Rest.Response;

/// <summary>
/// Represents the result of an aggregation operation.
/// </summary>
public class AggregationResult
{
    /// <summary>
    /// Gets or sets the value indicating how the aggregation was performed.
    /// </summary>
    public string AggregatedBy { get; set; }

    /// <summary>
    /// Gets or sets the type of the aggregation.
    /// </summary>
    public AggregationType Type { get; set; }

    /// <summary>
    /// Gets or sets the result of the aggregation.
    /// </summary>
    public object Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregationResult"/> class.
    /// </summary>
    public AggregationResult()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregationResult"/> class with the specified parameters.
    /// </summary>
    /// <param name="aggregatedBy">The value indicating how the aggregation was performed.</param>
    /// <param name="aggregationType">The type of the aggregation.</param>
    /// <param name="result">The result of the aggregation.</param>
    public AggregationResult(string aggregatedBy, AggregationType aggregationType, object result)
    {
        AggregatedBy = aggregatedBy;
        Type = aggregationType;
        Result = result;
    }
}
