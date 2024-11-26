using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Utils;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the header to a tenant identifier
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="HostResolutionStrategy"/>
/// </remarks>
/// <param name="httpContextAccessor"></param>
public class HeaderResolutionStrategy(IHttpContextAccessor httpContextAccessor) : ITenantResolutionStrategy<TenantId>
{
    /// <summary>
    /// Header key.
    /// </summary>
    public static string HeaderKey { get; set; } = TenancyConstants.HttpContextTenantKey;

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns></returns>
    public async Task<TenantId> GetTenantIdentifierAsync()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(HeaderKey, out StringValues tenantId) == true)
            return await Task.FromResult(TenantId.Parse(tenantId)).ConfigureAwait(false);

        return TenantId.Empty;
    }

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns></returns>
    public TenantId GetTenantIdentifier()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(HeaderKey, out StringValues tenantId) == true)
            return TenantId.Parse(tenantId);

        return TenantId.Empty;
    }
}
