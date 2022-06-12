using Microsoft.AspNetCore.Http;
using Milvasoft.Core.EntityBase.MultiTenancy;
using Milvasoft.MultiTenancy.Utils;
using System.Threading.Tasks;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the header to a tenant identifier
/// </summary>
public class HeaderResolutionStrategy : ITenantResolutionStrategy<TenantId>
{
    /// <summary>
    /// Header key.
    /// </summary>
    public static string HeaderKey = TenancyConstants.HttpContextTenantKey;

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates new instance of <see cref="HostResolutionStrategy"/>
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public HeaderResolutionStrategy(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns></returns>
    public async Task<TenantId> GetTenantIdentifierAsync()
    {
        var keyExist = await Task.FromResult(_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey(HeaderKey)).ConfigureAwait(false);

        return keyExist.GetValueOrDefault() ? await Task.FromResult(TenantId.Parse(_httpContextAccessor.HttpContext?.Request.Headers[HeaderKey])) : TenantId.Empty;
    }
}
