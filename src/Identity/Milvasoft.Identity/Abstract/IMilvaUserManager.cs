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
    /// Check password hash.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
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
    /// Generates a token for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the token will be for.</param>
    /// <param name="useUtcForDateTimes"></param>
    /// <param name="value"></param>
    /// <param name="user">The user the token will be for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents result of the asynchronous operation, a token for
    /// the given user and purpose.
    /// </returns>
    string GenerateUserToken(TUser user, Purpose purpose, bool useUtcForDateTimes, string value = null);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <param name="useUtcForDateTimes"></param>
    /// <param name="value"></param>
    /// <returns>
    /// True if the <paramref name="token"/>
    /// is valid, otherwise false.
    /// </returns>
    bool VerifyUserToken(TUser user, Purpose purpose, string token, bool useUtcForDateTimes, string value = null);
}