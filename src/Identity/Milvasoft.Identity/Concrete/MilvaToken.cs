using Milvasoft.Identity.Abstract;
using System.Text.Json.Serialization;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Represents token.
/// </summary>
public record MilvaToken : IToken
{
    /// <summary>
    /// Gets or sets the access token if the result is successful.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public DateTime Expiration { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the refresh token.
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public DateTime RefreshTokenExpiration { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the type of the token.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the session state.
    /// </summary>
    [JsonPropertyName("session_state")]
    public string SessionState { get; set; }

    /// <summary>
    /// Gets or sets the scope of the token.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
