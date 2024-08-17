namespace Milvasoft.Attributes.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class LinkedWithAttribute<TFormatter>(string propertyName, string serviceCollectionKey) : LinkedWithAttribute(propertyName, serviceCollectionKey) where TFormatter : ILinkedWithFormatter
{
    public Type FormatterType { get => typeof(TFormatter); }

    public string ServiceCollectionKey { get; set; } = serviceCollectionKey;
    public string PropertyName { get; set; } = propertyName;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class LinkedWithAttribute(string propertyName, string serviceCollectionKey) : Attribute
{
    public string ServiceCollectionKey { get; set; } = serviceCollectionKey;
    public string PropertyName { get; set; } = propertyName;
}
