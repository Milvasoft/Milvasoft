using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.TokenProvider;

namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Implements the standard Identity password hashing
/// </summary>
public interface IMilvaUserManager<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Sets password hash, normalized columns etc.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    void ConfigureForCreate(ref TUser user, string password);

    /// <summary>
    /// Checks <paramref name="password"/> against the user's password hash.
    /// If the password is correct but the stored hash was created with weaker hasher settings than the configured ones,
    /// <see cref="MilvaUser{TKey}.PasswordHash"/> is replaced with a new hash. Persist the user to keep the upgraded hash.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns>True if the password is correct.</returns>
    bool CheckPassword(TUser user, string password);

    /// <summary>
    /// Hashes password and sets user's password hash property.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    void SetPasswordHash(TUser user, string password);

    /// <summary>
    /// Validates and hashes password and sets user's password hash property. If validation failed throws exception.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    void ValidateAndSetPasswordHash(TUser user, string password);

    /// <summary>
    /// Configures user's lockout. Use before login.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="accessFailed"></param>
    /// <returns> If user locked returns true. </returns>
    bool ConfigureLockout(TUser user, bool accessFailed);

    /// <summary>
    /// User confirmed props and locked check.
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns></returns>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    string CheckPreLogin(TUser user);

    /// <summary>
    /// User confirmed props and locked check. 
    /// </summary>
    /// <param name="user">The user</param>
    void CheckPreLoginAndThrowIfInvalid(TUser user);

    /// <summary>
    /// User confirmed props check.
    /// </summary>
    /// <param name="user">The user whose sign-in status should be returned.</param>
    /// <returns> </returns>
    bool CanSignIn(TUser user);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> his locked out.
    /// </summary>
    /// <param name="user">The user whose locked out status should be retrieved.</param>
    /// <returns>
    /// True if the specified <paramref name="user "/> is locked out, otherwise false.
    /// </returns>
    bool IsLockedOut(TUser user);

    /// <summary>
    /// Validates user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    string ValidateUser(TUser user, string password = null);

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password supplied for validation</param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    string ValidatePassword(string password);

    /// <summary>
    /// Validates username.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    string ValidateUserName(string userName);

    /// <summary>
    /// Validates user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    void ThrowIfUserInvalid(TUser user, string password = null);

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password supplied for validation</param>
    /// <returns></returns>
    void ThrowIfPasswordIsInvalid(string password);

    /// <summary>
    /// Validates username.
    /// </summary>
    /// <param name="userName"></param>
    void ThrowIfUserNameIsInvalid(string userName);

    /// <summary>
    /// Validates email and returns error message key if invalid. If valid returns null.
    /// </summary>
    /// <param name="email"></param>
    string ValidateEmail(string email);

    /// <summary>
    /// Validates email.
    /// </summary>
    /// <param name="email"></param>
    void ThrowIfEmailIsInvalid(string email);

    /// <summary>
    /// Generates a data protection token for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// The token is bound to the user's security stamp (see <see cref="UserSecurityStamp"/>) and expires after one day.
    /// </summary>
    /// <param name="user">The user the token will be for.</param>
    /// <param name="purpose">The purpose the token will be for.</param>
    /// <param name="value">Optional value the token is bound to (for example the new email address in an email change flow).</param>
    /// <returns>A token for the given user and purpose.</returns>
    string GenerateUserToken(TUser user, Purpose purpose, string value = null);

    /// <summary>
    /// Generates a token for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user the token will be for.</param>
    /// <param name="purpose">The purpose the token will be for.</param>
    /// <param name="useUtcForDateTimes">Ignored. Tokens always use UTC.</param>
    /// <param name="value">Optional value the token is bound to.</param>
    /// <returns>A token for the given user and purpose.</returns>
    [Obsolete("Tokens always use UTC. Use GenerateUserToken(user, purpose, value) instead.")]
    string GenerateUserToken(TUser user, Purpose purpose, bool useUtcForDateTimes, string value = null) => GenerateUserToken(user, purpose, value);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <param name="value">The value the token was bound to when it was generated.</param>
    /// <returns>True if the <paramref name="token"/> is valid, otherwise false.</returns>
    bool VerifyUserToken(TUser user, Purpose purpose, string token, string value = null);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <param name="useUtcForDateTimes">Ignored. Tokens always use UTC.</param>
    /// <param name="value">The value the token was bound to when it was generated.</param>
    /// <returns>True if the <paramref name="token"/> is valid, otherwise false.</returns>
    [Obsolete("Tokens always use UTC. Use VerifyUserToken(user, purpose, token, value) instead.")]
    bool VerifyUserToken(TUser user, Purpose purpose, string token, bool useUtcForDateTimes, string value = null) => VerifyUserToken(user, purpose, token, value);

    /// <summary>
    /// Generates a 6 digit time based code for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// The code is derived from the configured server-side secret (<c>Token.DigitTokenSecret</c>) and the user's security stamp.
    /// </summary>
    /// <param name="user">The user the code will be for.</param>
    /// <param name="purpose">The purpose the code will be for.</param>
    /// <param name="value">Optional value the code is bound to (for example the new email address in an email change flow).</param>
    /// <returns>6 digit code.</returns>
    string GenerateDigitToken(TUser user, Purpose purpose, string value = null);

    /// <summary>
    /// Returns a flag indicating whether the specified 6 digit <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// Callers must rate limit validation attempts.
    /// </summary>
    /// <param name="user">The user to validate the code against.</param>
    /// <param name="purpose">The purpose the code was generated for.</param>
    /// <param name="token">The code to validate.</param>
    /// <param name="value">The value the code was bound to when it was generated.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    bool VerifyDigitToken(TUser user, Purpose purpose, string token, string value = null);
}