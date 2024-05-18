using Microsoft.IdentityModel.Tokens;

namespace Milvasoft.Identity.TokenProvider.AuthToken;

public class TokenConfig
{
    public int ExpirationMinute { get; set; }
    public SecurityKeyType SecurityKeyType { get; set; }
    public TokenValidationParameters TokenValidationParameters { get; set; }
    public string SymmetricPublicKey { get; set; }
    public RsaPublicKey RsaPublicKey { get; set; }

    internal SecurityKey GetSecurityKey() => SecurityKeyType switch
    {
        SecurityKeyType.Symmetric => GetSymmetricSecurityKey(SymmetricPublicKey),
        SecurityKeyType.Rsa => GetRSASecurityKey(RsaPublicKey.ToJson()),
        _ => null
    };

    internal string GetSecurityAlgorithm() => SecurityKeyType switch
    {
        SecurityKeyType.Symmetric => SecurityAlgorithms.HmacSha256Signature,
        SecurityKeyType.Rsa => SecurityAlgorithms.RsaSha256Signature,
        _ => null
    };

    internal static JsonWebKey GetRSASecurityKey(string json) => new(json);
    internal static SymmetricSecurityKey GetSymmetricSecurityKey(string key) => new(Encoding.UTF8.GetBytes(key));
}