using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the host to a tenant identifier
/// </summary>
public class HostResolutionStrategy : ITenantResolutionStrategy<string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates new instance of <see cref="HostResolutionStrategy"/>
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public HostResolutionStrategy(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get the tenant identifier from host.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetTenantIdentifierAsync()
    {
        return await Task.FromResult(_httpContextAccessor.HttpContext.Request.Host.Host);
    }
}
