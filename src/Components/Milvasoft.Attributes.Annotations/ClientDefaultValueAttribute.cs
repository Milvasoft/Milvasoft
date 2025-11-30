namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Specifies the default value for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ClientDefaultValueAttribute(object value) : Attribute
{
    /// <summary>
    /// Default value of the property.
    /// </summary>
    public object Value { get; } = value;
}