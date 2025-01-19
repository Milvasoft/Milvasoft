namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether the relevant field is aggregatable.
/// Default: true 
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AggregatableAttribute(bool aggregatable = true) : Attribute
{
    /// <summary>
    /// Gets or sets aggregatable.
    /// </summary>
    public bool Aggregatable { get; set; } = aggregatable;
}
