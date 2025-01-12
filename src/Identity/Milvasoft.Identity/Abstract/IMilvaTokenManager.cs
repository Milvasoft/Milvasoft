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
    /// <param name="issuer"></param>
    /// <returns></returns>
    public ClaimsPrincipal GetClaimsPrincipalIfValid(string token, string issuer);

    /// <summary>
    /// Overrides token valid issuer configuration in milva identity options for the next operation.
    /// </summary>
    /// <param name="issuer"></param>
    public void OverrideValidIssuer(string issuer);

    /// <summary>
    /// Generates token with expired claim.
    /// If expired parameter is null, default expired time is 10 minutes.
    /// </summary>
    public string GenerateToken(DateTime? expired = null, string issuer = null, params Claim[] claims);
}