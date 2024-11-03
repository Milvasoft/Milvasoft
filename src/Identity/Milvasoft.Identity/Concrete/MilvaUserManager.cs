using Microsoft.AspNetCore.DataProtection;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider;
using System.Text.RegularExpressions;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Implements the standard Identity password hashing.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="MilvaUserManager{TUser,TKEy}"/>/
/// </remarks>
/// <param name="dataProtector"></param>
/// <param name="options"></param>
/// <param name="passwordHasher"></param>
public partial class MilvaUserManager<TUser, TKey>(Lazy<IDataProtectionProvider> dataProtector,
                                                   MilvaIdentityOptions options,
                                                   Lazy<IMilvaPasswordHasher> passwordHasher) : IMilvaUserManager<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    private readonly Lazy<IDataProtectionProvider> _dataProtector = dataProtector;
    private readonly Lazy<IMilvaPasswordHasher> _passwordHasher = passwordHasher;
    private readonly MilvaIdentityOptions _options = options;

    /// <summary>
    /// Sets password hash, normalized columns etc.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public virtual void ConfigureForCreate(ref TUser user, string password)
    {
        user.PasswordHash = _passwordHasher.Value.HashPassword(password);

        if (_options.Lockout.AllowedForNewUsers)
            user.LockoutEnabled = true;

        user.NormalizedUserName = user.UserName.MilvaNormalize();
        user.NormalizedEmail = user.Email.MilvaNormalize();
    }

    /// <summary>
    /// Check password hash.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    public virtual bool CheckPassword(TUser user, string password)
        => _passwordHasher.Value.VerifyHashedPassword(user.PasswordHash, password);

    /// <summary>
    /// Hashes password and sets user's password hash property.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    public virtual void SetPasswordHash(TUser user, string password)
        => user.PasswordHash = _passwordHasher.Value.HashPassword(password);

    /// <summary>
    /// Validates and hashes password and sets user's password hash property. If validation failed throws exception.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password"></param>
    /// <returns></returns>
    public virtual void ValidateAndSetPasswordHash(TUser user, string password)
    {
        ThrowIfPasswordIsInvalid(password);

        SetPasswordHash(user, password);
    }

    /// <summary>
    /// Configures user's lockout. Use before login.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="accessFailed"></param>
    /// <returns> If user locked returns true. </returns>
    public virtual bool ConfigureLockout(TUser user, bool accessFailed)
    {
        if (user.LockoutEnabled)
        {
            if (accessFailed)
            {
                user.AccessFailedCount++;

                if (user.AccessFailedCount >= _options.Lockout.MaxFailedAccessAttempts)
                {
                    user.LockoutEnd = CommonHelper.GetDateTimeOffsetNow(_options.Token.UseUtcForDateTimes).Add(_options.Lockout.GetLockoutMinute());
                    user.AccessFailedCount = 0;

                    return true;
                }
            }
            else
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;

                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// User confirmed props and locked check.
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns></returns>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    public virtual string CheckPreLogin(TUser user)
    {
        if (!CanSignIn(user))
            return LocalizerKeys.NotAllowed;

        if (IsLockedOut(user))
            return LocalizerKeys.NotAllowed;

        return null;
    }

    /// <summary>
    /// User confirmed props and locked check. 
    /// </summary>
    /// <param name="user">The user</param>
    public virtual void CheckPreLoginAndThrowIfInvalid(TUser user)
    {
        if (!CanSignIn(user))
            MilvaIdentityExceptionThrower.ThrowNotAllowed();

        if (IsLockedOut(user))
            MilvaIdentityExceptionThrower.ThrowLocked();
    }

    /// <summary>
    /// User confirmed props check.
    /// </summary>
    /// <param name="user">The user whose sign-in status should be returned.</param>
    /// <returns> </returns>
    public virtual bool CanSignIn(TUser user)
    {
        if (_options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            return false;

        if (_options.SignIn.RequireConfirmedPhoneNumber && !user.PhoneNumberConfirmed)
            return false;

        return true;
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> his locked out.
    /// </summary>
    /// <param name="user">The user whose locked out status should be retrieved.</param>
    /// <returns>
    /// True if the specified <paramref name="user "/> is locked out, otherwise false.
    /// </returns>
    public virtual bool IsLockedOut(TUser user)
    {
        if (!user.LockoutEnabled)
            return false;

        var lockoutTime = user.LockoutEnd;

        return lockoutTime >= CommonHelper.GetDateTimeOffsetNow(_options.Token.UseUtcForDateTimes);
    }

    /// <summary>
    /// Validates user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    public virtual string ValidateUser(TUser user, string password = null)
    {
        var userNameValidaton = ValidateUserName(user.UserName);

        if (userNameValidaton != null)
            return userNameValidaton;

        if (_options.User.RequireUniqueEmail)
            return ValidateEmail(user.Email);

        if (password != null)
            return ValidatePassword(password);

        return null;
    }

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password supplied for validation</param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    public virtual string ValidatePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var passwordOptions = _options.Password;

        if (string.IsNullOrWhiteSpace(password) || password.Length < passwordOptions.RequiredLength)
            return LocalizerKeys.IdentityPasswordTooShort;

        if (passwordOptions.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
            return LocalizerKeys.IdentityPasswordRequiresNonAlphanumeric;

        if (passwordOptions.RequireDigit && !password.Any(IsDigit))
            return LocalizerKeys.IdentityPasswordRequiresDigit;

        if (passwordOptions.RequireLowercase && !password.Any(IsLower))
            return LocalizerKeys.IdentityPasswordRequiresLower;

        if (passwordOptions.RequireUppercase && !password.Any(IsUpper))
            return LocalizerKeys.IdentityPasswordRequiresUpper;

        if (passwordOptions.RequiredUniqueChars >= 1 && password.Distinct().Count() < passwordOptions.RequiredUniqueChars)
            return LocalizerKeys.IdentityPasswordRequiresUniqueChars;

        return null;
    }

    /// <summary>
    /// Validates username.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns> Returns error message key if invalid. If valid returns null. </returns>
    public virtual string ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return LocalizerKeys.IdentityInvalidUserName;

        if (!string.IsNullOrEmpty(_options.User.AllowedUserNameCharacters) && userName.Any(c => !_options.User.AllowedUserNameCharacters.Contains(c)))
            return LocalizerKeys.IdentityInvalidUserName;

        return null;
    }

    /// <summary>
    /// Validates user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    public virtual void ThrowIfUserInvalid(TUser user, string password = null)
    {
        var errorMessage = ValidateUser(user, password);

        if (errorMessage != null)
            throw new MilvaUserFriendlyException(errorMessage);
    }

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password supplied for validation</param>
    /// <returns></returns>
    public virtual void ThrowIfPasswordIsInvalid(string password)
    {
        var errorMessage = ValidatePassword(password);

        if (errorMessage != null)
            throw new MilvaUserFriendlyException(errorMessage);
    }

    /// <summary>
    /// Validates username.
    /// </summary>
    /// <param name="userName"></param>
    public virtual void ThrowIfUserNameIsInvalid(string userName)
    {
        var errorMessage = ValidateUserName(userName);

        if (errorMessage != null)
            throw new MilvaUserFriendlyException(errorMessage);
    }

    /// <summary>
    /// Validates email and returns error message key if invalid. If valid returns null.
    /// </summary>
    /// <param name="email"></param>
    public virtual string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return LocalizerKeys.InvalidEmail;

        if (!EmailRegex().IsMatch(email))
            return LocalizerKeys.IdentityInvalidUserName;

        return null;
    }

    /// <summary>
    /// Validates email.
    /// </summary>
    /// <param name="email"></param>
    public virtual void ThrowIfEmailIsInvalid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            MilvaIdentityExceptionThrower.ThrowInvalidEmail();

        if (!EmailRegex().IsMatch(email))
            MilvaIdentityExceptionThrower.ThrowInvalidEmail();
    }

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
    public virtual string GenerateUserToken(TUser user, Purpose purpose, bool useUtcForDateTimes, string value = null) => _dataProtector.Value.Generate(GetPurpose(user, purpose, value), user.Id, _options.Token.UseUtcForDateTimes);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <param name="value"></param>
    /// <param name="useUtcForDateTimes"></param>
    /// <returns>
    /// True if the <paramref name="token"/>
    /// is valid, otherwise false.
    /// </returns>
    public virtual bool VerifyUserToken(TUser user, Purpose purpose, string token, bool useUtcForDateTimes, string value = null) => _dataProtector.Value.Validate(GetPurpose(user, purpose, value), token, user.Id, useUtcForDateTimes);

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a digit.
    /// </summary>
    /// <param name="c">The character to check if it is a digit.</param>
    /// <returns>True if the character is a digit, otherwise false.</returns>
    protected virtual bool IsDigit(char c) => c >= '0' && c <= '9';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a lower case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is a lower case ASCII letter.</param>
    /// <returns>True if the character is a lower case ASCII letter, otherwise false.</returns>
    protected virtual bool IsLower(char c) => c >= 'a' && c <= 'z';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an upper case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is an upper case ASCII letter.</param>
    /// <returns>True if the character is an upper case ASCII letter, otherwise false.</returns>
    protected virtual bool IsUpper(char c) => c >= 'A' && c <= 'Z';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an ASCII letter or digit.
    /// </summary>
    /// <param name="c">The character to check if it is an ASCII letter or digit.</param>
    /// <returns>True if the character is an ASCII letter or digit, otherwise false.</returns>
    protected virtual bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);

    private static string GetPurpose(TUser user, Purpose purpose, string value = null) => purpose switch
    {
        Purpose.EmailChange => $"Email:ChangeEmail:{value}:{user.Email}",
        Purpose.EmailConfirm => $"Email:" + purpose + ":" + user.Email,
        Purpose.PasswordReset => $"Totp:" + purpose + ":" + user.Id,
        _ => string.Empty,
    };

    [GeneratedRegex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$")]
    private static partial Regex EmailRegex();
}
