namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether the relevant field is filterable.
/// Default: true 
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class FilterableAttribute(bool filterable = true) : Attribute
{
    /// <summary>
    /// Gets or sets filterable.
    /// </summary>
    public bool Filterable = filterable;
}
