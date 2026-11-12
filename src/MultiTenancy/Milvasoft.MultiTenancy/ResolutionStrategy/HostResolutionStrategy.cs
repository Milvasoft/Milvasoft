using Microsoft.AspNetCore.Http;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the host to a tenant identifier
/// </summary>
/// <remarks>
/// The host header is client controlled; the resolved value must be validated against a tenant store before it is trusted.
/// </remarks>
/// <param name="httpContextAccessor"></param>
public class HostResolutionStrategy(IHttpContextAccessor httpContextAccessor) : ITenantResolutionStrategy<string>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Get the tenant identifier from host.
    /// </summary>
    /// <returns>The host, or null when there is no request or host.</returns>
    public Task<string> GetTenantIdentifierAsync() => Task.FromResult(GetTenantIdentifier());

    /// <summary>
    /// Get the tenant identifier from host.
    /// </summary>
    /// <returns>The host, or null when there is no request or host.</returns>
    public string GetTenantIdentifier()
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host;

        return host?.HasValue == true ? host.Value.Host : null;
    }
}
