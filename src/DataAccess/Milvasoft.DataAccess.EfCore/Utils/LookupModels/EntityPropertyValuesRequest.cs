using Milvasoft.Components.Rest.Request;

namespace Milvasoft.DataAccess.EfCore.Utils.LookupModels;

/// <summary>
/// Model for when fetching entity's property values.
/// </summary>
public class EntityPropertyValuesRequest
{
    /// <summary>
    /// Entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Property name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Filter criterias.
    /// </summary>
    public FilterRequest Filtering { get; set; }

    /// <summary>
    /// Sort criterias.
    /// </summary>
    public SortRequest Sorting { get; set; }
}
