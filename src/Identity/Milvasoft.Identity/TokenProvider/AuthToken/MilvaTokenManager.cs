using Microsoft.IdentityModel.Tokens;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Milvasoft.Identity.TokenProvider.AuthToken;

/// <summary>
/// Token validation and generation manager.
/// </summary>
/// <param name="identityOptions"></param>
/// <param name="milvaLogger"></param>
public class MilvaTokenManager(MilvaIdentityOptions identityOptions, IMilvaLogger milvaLogger) : IMilvaTokenManager
{
    private readonly MilvaIdentityOptions _identityOptions = identityOptions;
    private readonly IMilvaLogger _milvaLogger = milvaLogger;

    /// <summary>
    /// Returns claims if token is valid.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public ClaimsPrincipal GetClaimsPrincipalIfValid(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            if (jwtToken == null)
                return null;

            var principal = tokenHandler.ValidateToken(token, _identityOptions.Token.TokenValidationParameters, out _);

            return principal;
        }
        catch (Exception ex)
        {
            _milvaLogger.Warning(ex, "Token validation error!");
            return null;
        }
    }

    /// <summary>
    /// Returns claims if token is valid.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="issuers"></param>
    /// <returns></returns>
    public ClaimsPrincipal GetClaimsPrincipalIfValid(string token, params string[] issuers)
    {
        OverrideValidIssuers(issuers);

        return GetClaimsPrincipalIfValid(token);
    }

    /// <summary>
    /// Overrides token valid issuers configuration in milva identity options.
    /// </summary>
    /// <param name="issuers"></param>
    public void OverrideValidIssuers(params string[] issuers) => _identityOptions.Token.TokenValidationParameters.ValidIssuers = issuers;

    /// <summary>
    /// Generates token with expired claim.
    /// If expired parameter is null, default expired time is 10 minutes.
    /// </summary>
    public string GenerateToken(DateTime? expired = null, params Claim[] claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claimsIdentityList = new ClaimsIdentity();

        var now = CommonHelper.GetNow(_identityOptions.Token.UseUtcForDateTimes);

        expired ??= now.AddMinutes(_identityOptions.Token.ExpirationMinute);

        claimsIdentityList.AddClaim(new Claim(ClaimTypes.Expired, value: expired.ToString()));
        claimsIdentityList.AddClaims(claims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentityList,
            Expires = expired,
            NotBefore = now,
            SigningCredentials = new SigningCredentials(_identityOptions.Token.GetSecurityKey(), _identityOptions.Token.GetSecurityAlgorithm())
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}