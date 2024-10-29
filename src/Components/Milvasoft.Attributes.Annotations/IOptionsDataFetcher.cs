namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Metadata options fetcher.
/// </summary>
public interface IOptionsDataFetcher
{
    /// <summary>
    /// Determines whether the fetcher is asynchronous.
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Fetch values.
    /// </summary>
    /// <returns></returns>
    public Task<List<object>> FetchAsync(object optionalData = null);

    /// <summary>
    /// Fetch values.
    /// </summary>
    /// <returns></returns>
    public List<object> Fetch(object optionalData = null);
}
