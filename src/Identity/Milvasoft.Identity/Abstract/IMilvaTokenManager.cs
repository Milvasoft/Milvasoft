using System.Security.Claims;

namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Token validation and generation manager.
/// </summary>
public interface IMilvaTokenManager
{
    /// <summary>
    /// Returns claims if token is valid.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public ClaimsPrincipal GetClaimsPrincipalIfValid(string token);

    /// <summary>
    /// Returns claims if token is valid.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="issuers"></param>
    /// <returns></returns>
    public ClaimsPrincipal GetClaimsPrincipalIfValid(string token, params string[] issuers);

    /// <summary>
    /// Overrides token valid issuers configuration in milva identity options for the next operation.
    /// </summary>
    /// <param name="issuers"></param>
    public void OverrideValidIssuers(params string[] issuers);

    /// <summary>
    /// Generates token with expired claim.
    /// If expired parameter is null, default expired time is 10 minutes.
    /// </summary>
    public string GenerateToken(DateTime? expired = null, params Claim[] claims);
}