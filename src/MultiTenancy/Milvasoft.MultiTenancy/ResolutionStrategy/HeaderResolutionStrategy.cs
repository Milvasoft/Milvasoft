using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Utils;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the header to a tenant identifier
/// </summary>
/// <remarks>
/// The header value is not trusted by itself: the tenant middleware validates it against the tenant store (and the user's tenant claim when configured).
/// Read the validated tenant from <see cref="Context.ITenantContext{TTenant, TKey}"/> instead of calling this strategy directly.
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
    /// <returns>The identifier, or <see cref="TenantId.Empty"/> when the header is missing, repeated or malformed.</returns>
    public Task<TenantId> GetTenantIdentifierAsync() => Task.FromResult(GetTenantIdentifier());

    /// <summary>
    /// Get the tenant identifier from header.
    /// </summary>
    /// <returns>The identifier, or <see cref="TenantId.Empty"/> when the header is missing, repeated or malformed.</returns>
    public TenantId GetTenantIdentifier()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(HeaderKey, out StringValues values) != true || values.Count != 1)
            return TenantId.Empty;

        var value = values[0]?.Trim();

        return TenantId.TryParse(value) ? new TenantId(value) : TenantId.Empty;
    }
}
