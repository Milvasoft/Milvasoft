using Milvasoft.Components.Rest.Enums;

namespace Milvasoft.Components.Rest.Response;

public class AggregationResult
{
    public string AggregatedBy { get; set; }

    public AggregationType Type { get; set; }

    public object Result { get; set; }

    public AggregationResult()
    {

    }

    public AggregationResult(string aggregatedBy, AggregationType aggregationType, object result)
    {
        AggregatedBy = aggregatedBy;
        Type = aggregationType;
        Result = result;
    }
}
