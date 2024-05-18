namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Determines whether this attribute will exluded in metadata generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExcludeFromMetadataAttribute : Attribute
{
}
