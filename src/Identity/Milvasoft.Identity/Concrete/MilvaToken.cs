using Milvasoft.Identity.Abstract;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Token representation.
/// </summary>
public record MilvaToken : IToken
{
    /// <summary>
    /// Gets or sets the access token if the result is successful.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the access token. (second)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the refresh token. (second)
    /// </summary>
    public int RefreshTokenExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the type of the token.
    /// </summary>
    public string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the session state.
    /// </summary>
    public string SessionState { get; set; }

    /// <summary>
    /// Gets or sets the scope of the token.
    /// </summary>
    public string Scope { get; set; }
}
