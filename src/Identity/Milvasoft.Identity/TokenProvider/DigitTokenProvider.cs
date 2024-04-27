using Microsoft.AspNetCore.Identity;
using Milvasoft.Identity.Concrete.Entity;
using System.Globalization;

namespace Milvasoft.Identity.TokenProvider;

/// <summary>
/// Represents a token provider that generates time based codes using the user's security stamp.
/// </summary>
/// <typeparam name="TUser">The type encapsulating a user.</typeparam>
/// <typeparam name="TKey">The type user's key.</typeparam>
public static class DigitTokenProvider<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Generates a token for the specified <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the token will be used for.</param>
    /// <param name="user">The user a token should be generated for. </param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the token for the specified 
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </returns>
    /// <remarks>
    /// The <paramref name="purpose"/> parameter allows a token generator to be used for multiple types of token whilst
    /// insuring a token for one purpose cannot be used for another. For example if you specified a purpose of "Email" 
    /// and validated it with the same purpose a token with the purpose of TOTP would not pass the check even if it was
    /// for the same user.
    /// 
    /// Implementations of <see cref="IUserTwoFactorTokenProvider{TUser}"/> should validate that purpose is not null or empty to
    /// help with token separation.
    /// </remarks>
    public static string Generate(Purpose purpose, TUser user)
    {
        var token = Encoding.Unicode.GetBytes(user.Id.ToString());

        var modifier = GetUserModifier(purpose, user);

        return Rfc6238AuthenticationService.GenerateCode(token, modifier).ToString("D6", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the token will be used for.</param>
    /// <param name="token">The token to validate.</param>
    /// <param name="user">The user a token should be validated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the a flag indicating the result
    /// of validating the <paramref name="token"> for the specified </paramref><paramref name="user"/> and <paramref name="purpose"/>.
    /// The task will return true if the token is valid, otherwise false.
    /// </returns>
    public static bool Validate(Purpose purpose, string token, TUser user)
    {
        if (!int.TryParse(token, out int code))
            return false;

        var securityToken = Encoding.Unicode.GetBytes(Guid.NewGuid().ToString());

        var modifier = GetUserModifier(purpose, user);

        return securityToken != null && Rfc6238AuthenticationService.ValidateCode(securityToken, code, modifier);
    }

    /// <summary>
    /// Returns a constant, provider and user unique modifier used for entropy in generated tokens from user information.
    /// </summary>
    /// <param name="purpose">The purpose the token will be generated for.</param>
    /// <param name="user">The user a token should be generated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a constant modifier for the specified 
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </returns>
    private static string GetUserModifier(Purpose purpose, TUser user) => purpose switch
    {
        Purpose.EmailChange => $"Email:ChangeEmail:limon@limon:bugrakosen@windowslive.com",
        Purpose.EmailConfirm => $"Totp:" + purpose + ":" + user.PhoneNumber,
        Purpose.PasswordReset => $"PhoneNumber:" + purpose + ":" + string.Empty,
        _ => string.Empty,
    };
}
