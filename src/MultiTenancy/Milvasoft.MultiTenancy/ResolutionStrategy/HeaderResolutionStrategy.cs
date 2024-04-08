using Fody;
using Microsoft.AspNetCore.Http;
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
[ConfigureAwait(false)]
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
        var keyExist = await Task.FromResult(_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey(HeaderKey)).ConfigureAwait(false);

        return keyExist.GetValueOrDefault() ? await Task.FromResult(TenantId.Parse(_httpContextAccessor.HttpContext?.Request.Headers[HeaderKey])) : TenantId.Empty;
    }
}
