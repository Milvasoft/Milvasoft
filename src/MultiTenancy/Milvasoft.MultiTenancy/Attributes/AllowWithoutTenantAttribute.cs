namespace Milvasoft.MultiTenancy.Attributes;

/// <summary>
/// Endpoint metadata that makes the tenant optional for the endpoint (the request is not rejected when no tenant is sent).
/// A valid tenant is still resolved when it is sent.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AllowWithoutTenantAttribute : Attribute
{
}
