namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Token representation.
/// </summary>
public interface IToken
{
    /// <summary>
    /// Gets or sets the access token if the result is successful.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the access token.
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the refresh token.
    /// </summary>
    public DateTime RefreshTokenExpiration { get; set; }

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
