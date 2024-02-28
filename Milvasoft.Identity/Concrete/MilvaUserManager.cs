using Microsoft.AspNetCore.DataProtection;
using Milvasoft.Core;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete.Entity;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Implements the standard Identity password hashing.
/// </summary>
public class MilvaUserManager<TUser, TKey> : IMilvaUserManager<TUser, TKey> where TUser : MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    private readonly Lazy<IDataProtectionProvider> _dataProtector;
    private readonly Lazy<IMilvaPasswordHasher> _passwordHasher;
    private readonly MilvaIdentityOptions _options;
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    /// <summary>
    /// Creates a new instance of <see cref="MilvaUserManager{TUser,TKEy}"/>/
    /// </summary>
    /// <param name="dataProtector"></param>
    /// <param name="options"></param>
    /// <param name="passwordHasher"></param>
    public MilvaUserManager(Lazy<IDataProtectionProvider> dataProtector,
                            MilvaIdentityOptions options,
                            Lazy<IMilvaPasswordHasher> passwordHasher)
    {
        _dataProtector = dataProtector;
        _options = options;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Validates user, sets password hash, set normalized columns.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public TUser ConfigureForCreate(TUser user, string password)
    {
        ValidateUser(user, password);

        return user;
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
        ValidatePassword(password);

        user.PasswordHash = _passwordHasher.Value.HashPassword(password);
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
                    user.LockoutEnd = DateTimeOffset.UtcNow.Add(_options.Lockout.GetLockoutMinute());
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
    public virtual void PreLoginCheck(TUser user)
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

        return lockoutTime >= DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Validates user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    public void ValidateUser(TUser user, string password = null)
    {
        ValidateUserName(user.UserName);

        if (_options.User.RequireUniqueEmail)
            ValidateEmail(user.Email);

        if (password != null)
        {
            ValidatePassword(password);
            user.PasswordHash = _passwordHasher.Value.HashPassword(password);
        }

        if (_options.Lockout.AllowedForNewUsers)
            user.LockoutEnabled = true;

        user.NormalizedUserName = user.UserName.MilvaNormalize();
        user.NormalizedEmail = user.Email.MilvaNormalize();
    }

    /// <summary>
    /// Validates a password.
    /// </summary>
    /// <param name="password">The password supplied for validation</param>
    /// <returns></returns>
    public virtual void ValidatePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var options = _options.Password;

        if (string.IsNullOrWhiteSpace(password) || password.Length < options.RequiredLength)
            MilvaIdentityExceptionThrower.ThrowPasswordTooShort();

        if (options.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
            MilvaIdentityExceptionThrower.ThrowPasswordRequiresNonAlphanumeric();

        if (options.RequireDigit && !password.Any(IsDigit))
            MilvaIdentityExceptionThrower.ThrowPasswordRequiresDigit();

        if (options.RequireLowercase && !password.Any(IsLower))
            MilvaIdentityExceptionThrower.ThrowPasswordRequiresLower();

        if (options.RequireUppercase && !password.Any(IsUpper))
            MilvaIdentityExceptionThrower.ThrowPasswordRequiresUpper();

        if (options.RequiredUniqueChars >= 1 && password.Distinct().Count() < options.RequiredUniqueChars)
            MilvaIdentityExceptionThrower.ThrowPasswordRequiresUniqueChars();
    }

    /// <summary>
    /// Validates username.
    /// </summary>
    /// <param name="userName"></param>
    public virtual void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            MilvaIdentityExceptionThrower.ThrowInvalidUserName();

        if (!string.IsNullOrEmpty(_options.User.AllowedUserNameCharacters) && userName.Any(c => !_options.User.AllowedUserNameCharacters.Contains(c)))
            MilvaIdentityExceptionThrower.ThrowInvalidUserName();
    }

    /// <summary>
    /// Validates email.
    /// </summary>
    /// <param name="email"></param>
    public virtual void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            MilvaIdentityExceptionThrower.ThrowInvalidEmail();

        if (!_emailAddressAttribute.IsValid(email))
            MilvaIdentityExceptionThrower.ThrowInvalidEmail();

    }

    /// <summary>
    /// Generates a token for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="purpose">The purpose the token will be for.</param>
    /// <param name="value"></param>
    /// <param name="user">The user the token will be for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents result of the asynchronous operation, a token for
    /// the given user and purpose.
    /// </returns>
    public virtual string GenerateUserToken(TUser user, Purpose purpose, string value = null) => _dataProtector.Value.Generate(GetPurpose(user, purpose, value), user.Id);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <param name="value"></param>
    /// <returns>
    /// True if the <paramref name="token"/>
    /// is valid, otherwise false.
    /// </returns>
    public virtual bool VerifyUserToken(TUser user, Purpose purpose, string token, string value = null) => _dataProtector.Value.Validate(GetPurpose(user, purpose, value), token, user.Id);

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a digit.
    /// </summary>
    /// <param name="c">The character to check if it is a digit.</param>
    /// <returns>True if the character is a digit, otherwise false.</returns>
    protected bool IsDigit(char c) => c >= '0' && c <= '9';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a lower case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is a lower case ASCII letter.</param>
    /// <returns>True if the character is a lower case ASCII letter, otherwise false.</returns>
    protected bool IsLower(char c) => c >= 'a' && c <= 'z';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an upper case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is an upper case ASCII letter.</param>
    /// <returns>True if the character is an upper case ASCII letter, otherwise false.</returns>
    protected bool IsUpper(char c) => c >= 'A' && c <= 'Z';

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an ASCII letter or digit.
    /// </summary>
    /// <param name="c">The character to check if it is an ASCII letter or digit.</param>
    /// <returns>True if the character is an ASCII letter or digit, otherwise false.</returns>
    protected bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);

    private static string GetPurpose(TUser user, Purpose purpose, string value = null) => purpose switch
    {
        Purpose.EmailChange => $"Email:ChangeEmail:{value}:{user.Email}",
        Purpose.EmailConfirm => $"Email:" + purpose + ":" + user.Email,
        Purpose.PasswordReset => $"Totp:" + purpose + ":" + user.Id,
        _ => string.Empty,
    };
}
