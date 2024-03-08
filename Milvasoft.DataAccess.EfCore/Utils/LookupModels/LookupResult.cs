namespace Milvasoft.DataAccess.EfCore.Utils.LookupModels;

/// <summary>
/// For lookup requests. 
/// </summary>
public class LookupResult
{
    /// <summary>
    /// Lookup key. (e.g. Product)
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Result list.
    /// </summary>
    public List<object> Data { get; set; }
}
