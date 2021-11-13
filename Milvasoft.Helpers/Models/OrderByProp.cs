using Milvasoft.Helpers.Attributes.Validation;

namespace Milvasoft.Helpers.Models;

/// <summary>
/// Order by properties for multiple ordey by in database.
/// </summary>
public class OrderByProp
{
    /// <summary>
    /// Determines order by Property name of entity.
    /// </summary>
    [ValidateString(1, 100)]
    public string PropName { get; set; }

    /// <summary>
    /// Determines order by ascending or descending.
    /// </summary>
    public bool Ascending { get; set; }

    /// <summary>
    /// Priority of order operation. Ex. first order by creation date then order by updated date.
    /// </summary>
    public int Priority { get; set; }
}
