namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Links property with another property and act with formatter.
/// </summary>
/// <typeparam name="TFormatter"></typeparam>
/// <param name="propertyName"></param>
/// <param name="serviceCollectionKey"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class LinkedWithAttribute<TFormatter>(string propertyName, string serviceCollectionKey) : LinkedWithAttribute(propertyName, serviceCollectionKey) where TFormatter : ILinkedWithFormatter
{
    /// <summary>
    /// Formatter type.
    /// </summary>
    public Type FormatterType { get => typeof(TFormatter); }
}

/// <summary>
/// Links property with another property and act with formatter.
/// </summary>
/// <param name="propertyName"></param>
/// <param name="serviceCollectionKey"></param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class LinkedWithAttribute(string propertyName, string serviceCollectionKey) : Attribute
{
    /// <summary>
    /// Formatter service collection key.
    /// </summary>
    public string ServiceCollectionKey { get; set; } = serviceCollectionKey;

    /// <summary>
    /// Linked property name.
    /// </summary>
    public string PropertyName { get; set; } = propertyName;
}
