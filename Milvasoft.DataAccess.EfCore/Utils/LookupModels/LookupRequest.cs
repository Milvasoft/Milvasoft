using Milvasoft.Components.Rest.Request;

namespace Milvasoft.DataAccess.EfCore.Utils.LookupModels;

/// <summary>
/// For request content. 
/// </summary>
public class LookupRequest
{
    public List<LookupRequestParameter> Parameters { get; set; }
}

public class LookupRequestParameter
{
    /// <summary>
    /// Requested entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Requested properties.
    /// </summary>
    public List<string> RequestedPropertyNames { get; set; }

    /// <summary>
    /// Filter criterias.
    /// </summary>
    public FilterRequest Filtering { get; set; }

    /// <summary>
    /// Sort criterias.
    /// </summary>
    public SortRequest Sorting { get; set; }
}