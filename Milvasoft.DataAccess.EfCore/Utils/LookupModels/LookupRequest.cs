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
    public ListRequest ListRequest { get; set; }

    /// <summary>
    /// Requested entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Requested properties.
    /// </summary>
    public List<string> RequestedPropertyNames { get; set; }
}