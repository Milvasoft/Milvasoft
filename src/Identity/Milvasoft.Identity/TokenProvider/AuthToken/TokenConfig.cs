using Microsoft.IdentityModel.Tokens;

namespace Milvasoft.Identity.TokenProvider.AuthToken;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class TokenConfig
{
    public bool UseUtcForDateTimes { get; set; }
    public int ExpirationMinute { get; set; }
    public SecurityKeyType SecurityKeyType { get; set; }
    public TokenValidationParameters TokenValidationParameters { get; set; }
    public string SymmetricPublicKey { get; set; }
    public RsaPublicKey RsaPublicKey { get; set; }

    public SecurityKey GetSecurityKey() => SecurityKeyType switch
    {
        SecurityKeyType.Symmetric => GetSymmetricSecurityKey(SymmetricPublicKey),
        SecurityKeyType.Rsa => GetRSASecurityKey(RsaPublicKey.ToJson()),
        _ => null
    };

    public string GetSecurityAlgorithm() => SecurityKeyType switch
    {
        SecurityKeyType.Symmetric => SecurityAlgorithms.HmacSha256Signature,
        SecurityKeyType.Rsa => SecurityAlgorithms.RsaSha256Signature,
        _ => null
    };

    internal static JsonWebKey GetRSASecurityKey(string json) => new(json);
    internal static SymmetricSecurityKey GetSymmetricSecurityKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member