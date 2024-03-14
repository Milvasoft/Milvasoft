namespace Milvasoft.DataAccess.EfCore.Utils.LookupModels;

/// <summary>
/// Lookup result model.
/// </summary>
public class LookupModel
{
    /// <summary>
    /// Name of lookup property
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Value of lookup property.
    /// </summary>
    public object PropertyValue { get; set; }
}