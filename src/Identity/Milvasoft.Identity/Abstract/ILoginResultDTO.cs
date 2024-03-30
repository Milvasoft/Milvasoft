using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Login result DTO for DI.
/// </summary>
public interface ILoginResultDto<TToken> where TToken : IToken
{
    /// <summary>
    /// Error messages of result.
    /// </summary>
    public List<IdentityError> ErrorMessages { get; set; }

    /// <summary>
    /// If result is success sets the token.
    /// </summary>
    public TToken Token { get; set; }
}
