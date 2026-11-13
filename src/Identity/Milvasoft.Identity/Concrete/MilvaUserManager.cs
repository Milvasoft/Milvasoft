using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
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

    /// <inheritdoc/>
    public virtual void ConfigureForCreate(ref TUser user, string password)
    {
        user.PasswordHash = _passwordHasher.Value.HashPassword(password);

        if (_options.Lockout.AllowedForNewUsers)
            user.LockoutEnabled = true;

        user.NormalizedUserName = user.UserName.MilvaNormalize();
        user.NormalizedEmail = user.Email.MilvaNormalize();

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
    public virtual void RotateSecurityStamp(TUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.SecurityStamp = UserSecurityStamp.NewStamp();
    }

    /// <inheritdoc/>
    public virtual void SetEmail(TUser user, string email, bool emailConfirmed = false)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Email = email;
        user.NormalizedEmail = email.MilvaNormalize();
        user.EmailConfirmed = emailConfirmed;

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
    public virtual void ConfirmEmail(TUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.EmailConfirmed = true;

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
    public virtual void SetPhoneNumber(TUser user, string phoneNumber, bool phoneNumberConfirmed = false)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.PhoneNumber = phoneNumber;
        user.PhoneNumberConfirmed = phoneNumberConfirmed;

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
    public virtual void ConfirmPhoneNumber(TUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.PhoneNumberConfirmed = true;

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
    public virtual bool CheckPassword(TUser user, string password) => CheckPassword(user, password, out _);

    /// <inheritdoc/>
    public virtual bool CheckPassword(TUser user, string password, out bool rehashNeeded)
    {
        var result = _passwordHasher.Value.VerifyHashedPasswordWithRehashCheck(user.PasswordHash, password);

        rehashNeeded = result == PasswordVerificationResult.SuccessRehashNeeded;

        return result != PasswordVerificationResult.Failed;
    }

    /// <inheritdoc/>
    public virtual bool RehashPassword(TUser user, string password)
    {
        if (!CheckPassword(user, password))
            return false;

        user.PasswordHash = _passwordHasher.Value.HashPassword(password);

        return true;
    }

    /// <inheritdoc/>
    public virtual void SetPasswordHash(TUser user, string password)
    {
        user.PasswordHash = _passwordHasher.Value.HashPassword(password);

        RotateSecurityStamp(user);
    }

    /// <inheritdoc/>
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
        {
            var emailValidation = ValidateEmail(user.Email);

            if (emailValidation != null)
                return emailValidation;
        }

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
            return LocalizerKeys.IdentityInvalidEmail;

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

    /// <inheritdoc/>
    public virtual string GenerateUserToken(TUser user, Purpose purpose, string value = null)
        => _dataProtector.Value.Generate(GetPurpose(user, purpose, value), user.Id, UserSecurityStamp.ComputeString(user));

    /// <inheritdoc/>
    [Obsolete("Tokens always use UTC. Use GenerateUserToken(user, purpose, value) instead.")]
    public virtual string GenerateUserToken(TUser user, Purpose purpose, bool useUtcForDateTimes, string value = null) => GenerateUserToken(user, purpose, value);

    /// <inheritdoc/>
    public virtual bool VerifyUserToken(TUser user, Purpose purpose, string token, string value = null)
    {
        var isValid = _dataProtector.Value.Validate(GetPurpose(user, purpose, value), token, user.Id, UserSecurityStamp.ComputeString(user));

        if (isValid && IsSingleUse(purpose))
            RotateSecurityStamp(user);

        return isValid;
    }

    /// <inheritdoc/>
    [Obsolete("Tokens always use UTC. Use VerifyUserToken(user, purpose, token, value) instead.")]
    public virtual bool VerifyUserToken(TUser user, Purpose purpose, string token, bool useUtcForDateTimes, string value = null) => VerifyUserToken(user, purpose, token, value);

    /// <inheritdoc/>
    public virtual string GenerateDigitToken(TUser user, Purpose purpose, string value = null)
        => DigitTokenProvider<TUser, TKey>.Generate(purpose, user, _options.Token.GetDigitTokenSecret(), value);

    /// <inheritdoc/>
    public virtual bool VerifyDigitToken(TUser user, Purpose purpose, string token, string value = null)
    {
        var isValid = DigitTokenProvider<TUser, TKey>.Validate(purpose, token, user, _options.Token.GetDigitTokenSecret(), value);

        if (isValid && IsSingleUse(purpose))
            RotateSecurityStamp(user);

        return isValid;
    }

    /// <summary>
    /// Returns whether a verified token or code of <paramref name="purpose"/> is consumed by rotating the user's security stamp.
    /// All built-in purposes (email confirmation, email change, password reset) are single-use.
    /// </summary>
    /// <param name="purpose">The token purpose.</param>
    /// <returns>True if the token must not be accepted twice.</returns>
    protected virtual bool IsSingleUse(Purpose purpose) => purpose is Purpose.EmailConfirm or Purpose.EmailChange or Purpose.PasswordReset;

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
        Purpose.EmailConfirm => $"Email:{purpose}:{user.Email}",
        Purpose.PasswordReset => $"Totp:{purpose}{user.Id}",
        _ => string.Empty,
    };

    [GeneratedRegex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$")]
    private static partial Regex EmailRegex();
}
