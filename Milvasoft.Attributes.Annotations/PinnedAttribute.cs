namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether the relevant field is pinned.
/// Default: false 
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PinnedAttribute(bool pinned = true) : Attribute
{
    /// <summary>
    /// Gets or sets pinned.
    /// </summary>
    public bool Pinned = pinned;
}
