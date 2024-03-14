using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Provides localized identity error messages. All error's code is nameof method. (e.g. Code = "DefaultError")
/// </summary>
public static class MilvaIdentityExceptionThrower
{
    /// <summary>
    /// An unknown failure has occurred. Localizer Key : IdentityDefaultError
    /// </summary>
    /// <returns></returns>
    public static void ThrowDefaultError() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityDefaultError);

    /// <summary>
    /// There is another user with this username. Localizer Key : IdentityDuplicateUsername
    /// </summary>
    /// <returns></returns>
    public static void ThrowDuplicateUserName() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityDuplicateUsername);

    /// <summary>
    /// There is another user with this email. Localizer Key : IdentityDuplicateEmail
    /// </summary>
    /// <returns></returns>
    public static void ThrowDuplicateEmail() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityDuplicateEmail);

    /// <summary>
    /// Invalid username. Localizer Key : IdentityInvalidUsername
    /// </summary>
    /// <returns></returns>
    public static void ThrowInvalidUserName() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityInvalidUserName);

    /// <summary>
    /// Invalid email. Localizer Key : IdentityInvalidEmail
    /// </summary>
    /// <returns></returns>
    public static void ThrowInvalidEmail() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityInvalidEmail);

    /// <summary>
    /// Concurrency failure, data has been modified. Localizer Key : IdentityConcurrencyFailure
    /// </summary>
    /// <returns></returns>
    public static void ThrowConcurrencyFailure() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityConcurrencyFailure);

    /// <summary>
    /// This role is already exist. Localizer Key : IdentityDuplicateRoleName
    /// </summary>
    /// <returns></returns>
    public static void ThrowDuplicateRoleName() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityDuplicateRoleName);

    /// <summary>
    /// Invalid role name. Localizer Key : IdentityInvalidRoleName
    /// </summary>
    /// <returns></returns>
    public static void ThrowInvalidRoleName() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityInvalidRoleName);

    /// <summary>
    /// Invalid credentials. Localizer Key : IdentityInvalidToken
    /// </summary>
    /// <returns></returns>
    public static void ThrowInvalidToken() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityInvalidToken);

    /// <summary>
    /// This user is already logged in. Localizer Key : IdentityLoginAlreadyAssociated
    /// </summary>
    /// <returns></returns>
    public static void ThrowLoginAlreadyAssociated() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityLoginAlreadyAssociated);

    /// <summary>
    /// Incorrect password. Localizer Key : IdentityPasswordMismatch
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordMismatch() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordMismatch);

    /// <summary>
    /// Password must have at least one digit. Localizer Key : IdentityPasswordRequiresDigit
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordRequiresDigit() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordRequiresDigit);

    /// <summary>
    /// Password must have at least one lowercase. Localizer Key : IdentityPasswordRequiresLower
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordRequiresLower() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordRequiresLower);

    /// <summary>
    /// Password must have at least one alphanumeric character. Localizer Key : IdentityPasswordRequiresNonAlphanumeric
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordRequiresNonAlphanumeric() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordRequiresNonAlphanumeric);

    /// <summary>
    /// Password must have at least one special character. Localizer Key : IdentityPasswordRequiresUniqueChars
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordRequiresUniqueChars() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordRequiresUniqueChars);

    /// <summary>
    /// Password must have at least one uppercase. Localizer Key : IdentityPasswordRequiresUpper
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordRequiresUpper() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordRequiresUpper);

    /// <summary>
    /// Password lenght is too short. Localizer Key : IdentityPasswordTooShort
    /// </summary>
    /// <returns></returns>
    public static void ThrowPasswordTooShort() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityPasswordTooShort);

    /// <summary>
    /// Recovery failed. Localizer Key : IdentityRecoveryCodeRedemptionFailed
    /// </summary>
    /// <returns></returns>
    public static void ThrowRecoveryCodeRedemptionFailed() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityRecoveryCodeRedemptionFailed);

    /// <summary>
    /// User already has password. Localizer Key : IdentityUserAlreadyHasPassword
    /// </summary>
    /// <returns></returns>
    public static void ThrowUserAlreadyHasPassword() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityUserAlreadyHasPassword);

    /// <summary>
    /// The user is already assigned to this role. Localizer Key : IdentityUserAlreadyInRole
    /// </summary>
    /// <returns></returns>
    public static void ThrowUserAlreadyInRole() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityUserAlreadyInRole);

    /// <summary>
    /// Lockout is not enabled for this user. Localizer Key : IdentityUserLockoutNotEnabled
    /// </summary>
    /// <returns></returns>
    public static void ThrowUserLockoutNotEnabled() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityUserLockoutNotEnabled);

    /// <summary>
    /// User is not assigned to this role. Localizer Key : IdentityUserNotInRole
    /// </summary>
    /// <returns></returns>
    public static void ThrowUserNotInRole() => throw new MilvaUserFriendlyException(LocalizerKeys.IdentityUserNotInRole);

    /// <summary>
    /// User is not assigned to this role. Localizer Key : NotAllowed
    /// </summary>
    /// <returns></returns>
    public static void ThrowNotAllowed() => throw new MilvaUserFriendlyException(LocalizerKeys.NotAllowed);

    /// <summary>
    /// User is not assigned to this role. Localizer Key : Locked
    /// </summary>
    /// <returns></returns>
    public static void ThrowLocked() => throw new MilvaUserFriendlyException(LocalizerKeys.Locked);

    /// <summary>
    /// User is not assigned to this role. Localizer Key : Locked
    /// </summary>
    /// <returns></returns>
    public static void ThrowLockedWarning() => throw new MilvaUserFriendlyException(LocalizerKeys.LockWarning);

}
