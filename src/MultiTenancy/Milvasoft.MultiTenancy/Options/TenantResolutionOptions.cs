using Microsoft.AspNetCore.Http;

namespace Milvasoft.MultiTenancy.Options;

/// <summary>
/// Why the tenant middleware rejected a request.
/// </summary>
public enum TenantRejectionReason
{
    /// <summary>
    /// No (or a malformed) tenant identifier was sent.
    /// </summary>
    Missing,

    /// <summary>
    /// The tenant identifier does not belong to a known tenant.
    /// </summary>
    NotFound,

    /// <summary>
    /// The authenticated user's tenant claim does not match the requested tenant.
    /// </summary>
    ClaimMismatch,
}

/// <summary>
/// Options of the tenant middleware.
/// </summary>
public class TenantResolutionOptions
{
    /// <summary>
    /// Claim type that holds the tenant identifier of an authenticated user. Null (default) disables the check.
    /// When set, a request of an authenticated user is rejected with <see cref="TenantRejectionReason.ClaimMismatch"/>
    /// unless the claim value equals the resolved tenant identifier (<c>ToString()</c>, ordinal). Anonymous requests are not affected.
    /// </summary>
    public string TenantClaimType { get; set; }

    /// <summary>
    /// Requests for which a tenant is optional (for example health checks, static files or documentation).
    /// The tenant is still resolved when a valid identifier is sent. Endpoints marked with <see cref="Attributes.AllowWithoutTenantAttribute"/> are always optional.
    /// </summary>
    public Func<HttpContext, bool> IsTenantOptional { get; set; }

    /// <summary>
    /// Writes the response of a rejected request. When null, only the status code is set:
    /// 400 for <see cref="TenantRejectionReason.Missing"/>, 404 for <see cref="TenantRejectionReason.NotFound"/> and 403 for <see cref="TenantRejectionReason.ClaimMismatch"/>.
    /// </summary>
    public Func<HttpContext, TenantRejectionReason, Task> OnRejected { get; set; }
}
