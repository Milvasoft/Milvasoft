using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.TokenProvider;

namespace Milvasoft.Identity.Abstract;

/// <summary>
/// Implements the standard Identity password hashing
/// </summary>
public interface IMilvaUserManager<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Sets password hash, normalized columns, lockout and a new random <see cref="MilvaUser{TKey}.SecurityStamp"/>.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    void ConfigureForCreate(ref TUser user, string password);

    /// <summary>
    /// Assigns a new random <see cref="MilvaUser{TKey}.SecurityStamp"/> to <paramref name="user"/>. Every user token and 6 digit code issued to the user
    /// before stops validating once the user is persisted. Use it to revoke all outstanding tokens (for example when an account may be compromised).
    /// </summary>
    /// <remarks>
    /// The methods of this interface that change credentials or identity call it themselves. Call it yourself when you change
    /// such a value directly on the entity. Persist the user afterwards.
    /// </remarks>
    /// <param name="user">The user.</param>
    void RotateSecurityStamp(TUser user);

    /// <summary>
    /// Sets the email address of <paramref name="user"/> (and its normalized form), sets <see cref="MilvaUser{TKey}.EmailConfirmed"/>
    /// to <paramref name="emailConfirmed"/> and rotates the security stamp. Persist the user afterwards.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="email">The new email address.</param>
    /// <param name="emailConfirmed">
    /// Whether the new address is already confirmed, for example after a verified <see cref="Purpose.EmailChange"/> token. Default is false.
    /// </param>
    void SetEmail(TUser user, string email, bool emailConfirmed = false);

    /// <summary>
    /// Marks the email address of <paramref name="user"/> as confirmed and rotates the security stamp. Persist the user afterwards.
    /// </summary>
    /// <param name="user">The user.</param>
    void ConfirmEmail(TUser user);

    /// <summary>
    /// Sets the phone number of <paramref name="user"/>, sets <see cref="MilvaUser{TKey}.PhoneNumberConfirmed"/> to <paramref name="phoneNumberConfirmed"/>
    /// and rotates the security stamp. Persist the user afterwards.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="phoneNumber">The new phone number.</param>
    /// <param name="phoneNumberConfirmed">Whether the new number is already confirmed. Default is false.</param>
    void SetPhoneNumber(TUser user, string phoneNumber, bool phoneNumberConfirmed = false);

    /// <summary>
    /// Marks the phone number of <paramref name="user"/> as confirmed and rotates the security stamp. Persist the user afterwards.
    /// </summary>
    /// <param name="user">The user.</param>
    void ConfirmPhoneNumber(TUser user);

    /// <summary>
    /// Checks <paramref name="password"/> against the user's password hash. The user is not modified.
    /// </summary>
    /// <remarks>
    /// This overload does not report whether the stored hash should be upgraded. Use
    /// <see cref="CheckPassword(TUser, string, out bool)"/> on sign-in to upgrade hashes created with weaker hasher settings.
    /// Lockout state (<see cref="MilvaUser{TKey}.AccessFailedCount"/>, <see cref="MilvaUser{TKey}.LockoutEnd"/>) is not changed;
    /// use <see cref="ConfigureLockout(TUser, bool)"/> for that.
    /// </remarks>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns>True if the password is correct.</returns>
    bool CheckPassword(TUser user, string password);

    /// <summary>
    /// Checks <paramref name="password"/> against the user's password hash and reports whether the stored hash was created with
    /// weaker hasher settings than the configured ones. The user is not modified.
    /// </summary>
    /// <remarks>
    /// When the method returns true and <paramref name="rehashNeeded"/> is true, call <see cref="RehashPassword(TUser, string)"/>
    /// with the same password and persist the user to store the upgraded hash. Ignoring it is safe; the old hash keeps verifying.
    /// Lockout state is not changed; use <see cref="ConfigureLockout(TUser, bool)"/> for that.
    /// </remarks>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <param name="rehashNeeded">True if the password is correct and the stored hash should be replaced. Always false when the password is wrong.</param>
    /// <returns>True if the password is correct.</returns>
    bool CheckPassword(TUser user, string password, out bool rehashNeeded);

    /// <summary>
    /// Replaces the user's password hash with a new hash of the same <paramref name="password"/> created with the configured hasher settings.
    /// Use it after <see cref="CheckPassword(TUser, string, out bool)"/> reported that a rehash is needed, then persist the user.
    /// </summary>
    /// <remarks>
    /// The password is verified against the stored hash first, so this method cannot be used to set a different password
    /// (use <see cref="SetPasswordHash(TUser, string)"/> for password changes). This costs one extra hash computation, which only happens
    /// once per user after the hasher settings change. The security stamp is not rotated because the credential does not change.
    /// </remarks>
    /// <param name="user">The user</param>
    /// <param name="password">The user's current password.</param>
    /// <returns>True if the password was correct and the hash was replaced; false if the password is wrong (the user is not modified).</returns>
    bool RehashPassword(TUser user, string password);

    /// <summary>
    /// Hashes password, sets user's password hash property and rotates the security stamp (password set, change or reset).
    /// Persist the user afterwards.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    void SetPasswordHash(TUser user, string password);

    /// <summary>
    /// Validates and hashes password, sets user's password hash property and rotates the security stamp. If validation failed throws exception.
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
    /// The user is not modified, so the token can be generated without persisting the user.
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
    /// <remarks>
    /// Tokens of single-use purposes (all values of <see cref="Purpose"/>) are consumed: when the token is valid the security stamp of
    /// <paramref name="user"/> is rotated, which also revokes every other outstanding token and code of the user.
    /// Persist the user after a successful verification, otherwise the token stays usable until it expires.
    /// Verify a token once per operation; a second verification of the same token returns false.
    /// </remarks>
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
    /// The user is not modified.
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
    /// <remarks>
    /// Codes of single-use purposes (all values of <see cref="Purpose"/>) are consumed: when the code is valid the security stamp of
    /// <paramref name="user"/> is rotated, which also revokes every other outstanding token and code of the user.
    /// Persist the user after a successful verification, otherwise the code stays usable until its time window ends.
    /// </remarks>
    /// <param name="user">The user to validate the code against.</param>
    /// <param name="purpose">The purpose the code was generated for.</param>
    /// <param name="token">The code to validate.</param>
    /// <param name="value">The value the code was bound to when it was generated.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    bool VerifyDigitToken(TUser user, Purpose purpose, string token, string value = null);
}