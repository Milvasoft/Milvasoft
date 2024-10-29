namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Fetches options metadata.
/// </summary>
/// <typeparam name="TFetcher"></typeparam>
/// <param name="serviceCollectionKey"></param>
/// <param name="optionalData"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class OptionsAttribute<TFetcher>(string serviceCollectionKey, object optionalData = null) : OptionsAttribute(serviceCollectionKey, optionalData) where TFetcher : IOptionsDataFetcher
{
    /// <summary>
    /// Formatter type.
    /// </summary>
    public Type FetcherType { get => typeof(TFetcher); }
}

/// <summary>
/// Fetches options metadata.
/// </summary>
/// <param name="serviceCollectionKey"></param>
/// <param name="optionalData"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class OptionsAttribute(string serviceCollectionKey, object optionalData = null) : Attribute
{
    /// <summary>
    /// Fetcher service collection key.
    /// </summary>
    public string ServiceCollectionKey { get; set; } = serviceCollectionKey;

    /// <summary>
    /// Optional data.
    /// </summary>
    public object OptionalData { get; } = optionalData;
}
