namespace Milvasoft.Attributes.Annotations;

/// <summary>
/// Composite index with tenant id.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class WithTenantIdIndexAttribute : Attribute;