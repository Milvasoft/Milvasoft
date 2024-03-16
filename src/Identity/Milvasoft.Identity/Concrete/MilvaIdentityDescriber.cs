using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Provides localized identity error messages. All error's code is nameof(<see cref="IdentityError"/>) method. (e.g. Code = "DefaultError")
/// </summary>
public class MilvaIdentityDescriber : IdentityErrorDescriber
{
    private readonly IMilvaLocalizer _localizer;

    /// <summary>
    /// Constructor for localizer dependenct injection.
    /// </summary>
    /// <param name="localizer"></param>
    public MilvaIdentityDescriber(IMilvaLocalizer localizer) => _localizer = localizer;

    /// <summary>
    /// An unknown failure has occurred. Localizer Key : IdentityDefaultError
    /// </summary>
    /// <returns></returns>
    public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = _localizer[LocalizerKeys.IdentityDefaultError] };

    /// <summary>
    /// There is another user with this username. Localizer Key : IdentityDuplicateUsername
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public override IdentityError DuplicateUserName(string userName) => new() { Code = nameof(DuplicateUserName), Description = _localizer[LocalizerKeys.IdentityDuplicateUsername] };

    /// <summary>
    /// There is another user with this email. Localizer Key : IdentityDuplicateEmail
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public override IdentityError DuplicateEmail(string email) => new() { Code = nameof(DuplicateEmail), Description = _localizer[LocalizerKeys.IdentityDuplicateEmail] };

    /// <summary>
    /// Invalid username. Localizer Key : IdentityInvalidUsername
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public override IdentityError InvalidUserName(string userName) => new() { Code = nameof(InvalidUserName), Description = _localizer[LocalizerKeys.IdentityInvalidUserName] };

    /// <summary>
    /// Invalid email. Localizer Key : IdentityInvalidEmail
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public override IdentityError InvalidEmail(string email) => new() { Code = nameof(InvalidEmail), Description = _localizer[LocalizerKeys.IdentityInvalidEmail] };

    /// <summary>
    /// Concurrency failure, data has been modified. Localizer Key : IdentityConcurrencyFailure
    /// </summary>
    /// <returns></returns>
    public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = _localizer[LocalizerKeys.IdentityConcurrencyFailure] };

    /// <summary>
    /// This role is already exist. Localizer Key : IdentityDuplicateRoleName
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public override IdentityError DuplicateRoleName(string role) => new() { Code = nameof(DuplicateRoleName), Description = _localizer[LocalizerKeys.IdentityDuplicateRoleName] };

    /// <summary>
    /// Invalid role name. Localizer Key : IdentityInvalidRoleName
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public override IdentityError InvalidRoleName(string role) => new() { Code = nameof(InvalidRoleName), Description = _localizer[LocalizerKeys.IdentityInvalidRoleName] };

    /// <summary>
    /// Invalid credentials. Localizer Key : IdentityInvalidToken
    /// </summary>
    /// <returns></returns>
    public override IdentityError InvalidToken() => new() { Code = nameof(InvalidToken), Description = _localizer[LocalizerKeys.IdentityInvalidToken] };

    /// <summary>
    /// This user is already logged in. Localizer Key : IdentityLoginAlreadyAssociated
    /// </summary>
    /// <returns></returns>
    public override IdentityError LoginAlreadyAssociated() => new() { Code = nameof(LoginAlreadyAssociated), Description = _localizer[LocalizerKeys.IdentityLoginAlreadyAssociated] };

    /// <summary>
    /// Incorrect password. Localizer Key : IdentityPasswordMismatch
    /// </summary>
    /// <returns></returns>
    public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = _localizer[LocalizerKeys.IdentityPasswordMismatch] };

    /// <summary>
    /// Password must have at least one digit. Localizer Key : IdentityPasswordRequiresDigit
    /// </summary>
    /// <returns></returns>
    public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = _localizer[LocalizerKeys.IdentityPasswordRequiresDigit] };

    /// <summary>
    /// Password must have at least one lowercase. Localizer Key : IdentityPasswordRequiresLower
    /// </summary>
    /// <returns></returns>
    public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = _localizer[LocalizerKeys.IdentityPasswordRequiresLower] };

    /// <summary>
    /// Password must have at least one alphanumeric character. Localizer Key : IdentityPasswordRequiresNonAlphanumeric
    /// </summary>
    /// <returns></returns>
    public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer[LocalizerKeys.IdentityPasswordRequiresNonAlphanumeric] };

    /// <summary>
    /// Password must have at least one special character. Localizer Key : IdentityPasswordRequiresUniqueChars
    /// </summary>
    /// <param name="uniqueChars"></param>
    /// <returns></returns>
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new() { Code = nameof(PasswordRequiresUniqueChars), Description = _localizer[LocalizerKeys.IdentityPasswordRequiresUniqueChars] };

    /// <summary>
    /// Password must have at least one uppercase. Localizer Key : IdentityPasswordRequiresUpper
    /// </summary>
    /// <returns></returns>
    public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = _localizer[LocalizerKeys.IdentityPasswordRequiresUpper] };

    /// <summary>
    /// Password lenght is too short. Localizer Key : IdentityPasswordTooShort
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = _localizer[LocalizerKeys.IdentityPasswordTooShort] };

    /// <summary>
    /// Recovery failed. Localizer Key : IdentityRecoveryCodeRedemptionFailed
    /// </summary>
    /// <returns></returns>
    public override IdentityError RecoveryCodeRedemptionFailed() => new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = _localizer[LocalizerKeys.IdentityRecoveryCodeRedemptionFailed] };

    /// <summary>
    /// User already has password. Localizer Key : IdentityUserAlreadyHasPassword
    /// </summary>
    /// <returns></returns>
    public override IdentityError UserAlreadyHasPassword() => new() { Code = nameof(UserAlreadyHasPassword), Description = _localizer[LocalizerKeys.IdentityUserAlreadyHasPassword] };

    /// <summary>
    /// The user is already assigned to this role. Localizer Key : IdentityUserAlreadyInRole
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public override IdentityError UserAlreadyInRole(string role) => new() { Code = nameof(UserAlreadyInRole), Description = _localizer[LocalizerKeys.IdentityUserAlreadyInRole] };

    /// <summary>
    /// Lockout is not enabled for this user. Localizer Key : IdentityUserLockoutNotEnabled
    /// </summary>
    /// <returns></returns>
    public override IdentityError UserLockoutNotEnabled() => new() { Code = nameof(UserLockoutNotEnabled), Description = _localizer[LocalizerKeys.IdentityUserLockoutNotEnabled] };

    /// <summary>
    /// User is not assigned to this role. Localizer Key : IdentityUserNotInRole
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public override IdentityError UserNotInRole(string role) => new() { Code = nameof(UserNotInRole), Description = _localizer[LocalizerKeys.IdentityUserNotInRole] };

}
