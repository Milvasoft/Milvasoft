using Milvasoft.Components.Rest.Enums;

namespace Milvasoft.Components.Rest.Response;

public class AggregationResult(string aggregatedBy, AggregationType aggregationType, object result)
{
    public string AggregatedBy { get; set; } = aggregatedBy;

    public AggregationType Type { get; set; } = aggregationType;

    public object Result { get; set; } = result;
}
