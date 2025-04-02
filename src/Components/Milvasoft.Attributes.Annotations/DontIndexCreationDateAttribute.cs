namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Represents that the creation date of the entity should not be indexed.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class DontIndexCreationDateAttribute : Attribute;