namespace Milvasoft.Components.OpenApi.Attributes;

/// <summary>
/// Excludes property from openapi documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
public class OpenApiExcludeAttribute : Attribute
{
}
