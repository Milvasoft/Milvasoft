namespace Milvasoft.Core.Utils.Constants;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Localizer message keys
/// </summary>
public static class LocalizerKeys
{
    public const string DefaultErrorMessage = "An error occured while processing!";
    public const string InvalidUrlErrorMessage = "Invalid url! Please check url parts!";
    public const string Localized = nameof(Localized);
    public const string Successful = nameof(Successful);
    public const string Failed = nameof(Failed);
    public const string PleaseSelectAValid = nameof(PleaseSelectAValid);
    public const string PleaseEnterAValid = "Please enter a valid";
    public const string FileNotFound = nameof(FileNotFound);
    public const string FileShouldBeUploaded = nameof(FileShouldBeUploaded);
    public const string UnsupportedMediaType = nameof(UnsupportedMediaType);
    public const string UploadFailed = nameof(UploadFailed);

    #region Identity

    public const string RequiresTwoFactor = nameof(RequiresTwoFactor);
    public const string NotAllowed = nameof(NotAllowed);
    public const string CannotFindUserWithThisToken = nameof(CannotFindUserWithThisToken);
    public const string IncorrectOldPassword = nameof(IncorrectOldPassword);
    public const string Hours = nameof(Hours);
    public const string Minutes = nameof(Minutes);
    public const string Seconds = nameof(Seconds);
    public const string Locked = nameof(Locked);
    public const string PleaseEnterEmailOrUsername = nameof(PleaseEnterEmailOrUsername);
    public const string InvalidUserName = nameof(InvalidUserName);
    public const string InvalidEmail = nameof(InvalidEmail);
    public const string InvalidLogin = nameof(InvalidLogin);
    public const string LockWarning = nameof(LockWarning);
    public const string UserValidationUserNameNumberStartWith = nameof(UserValidationUserNameNumberStartWith);
    public const string UserValidationUserNameLength = nameof(UserValidationUserNameLength);
    public const string UserValidationEmailLength = nameof(UserValidationEmailLength);

    #region Identity Message Keys

    public const string IdentityDefaultError = nameof(IdentityDefaultError);
    public const string IdentityDuplicateUsername = nameof(IdentityDuplicateUsername);
    public const string IdentityDuplicateEmail = nameof(IdentityDuplicateEmail);
    public const string IdentityInvalidUserName = nameof(IdentityInvalidUserName);
    public const string IdentityInvalidEmail = nameof(IdentityInvalidEmail);
    public const string IdentityConcurrencyFailure = nameof(IdentityConcurrencyFailure);
    public const string IdentityDuplicateRoleName = nameof(IdentityDuplicateRoleName);
    public const string IdentityInvalidRoleName = nameof(IdentityInvalidRoleName);
    public const string IdentityInvalidToken = nameof(IdentityInvalidToken);
    public const string IdentityLoginAlreadyAssociated = nameof(IdentityLoginAlreadyAssociated);
    public const string IdentityPasswordMismatch = nameof(IdentityPasswordMismatch);
    public const string IdentityPasswordRequiresDigit = nameof(IdentityPasswordRequiresDigit);
    public const string IdentityPasswordRequiresLower = nameof(IdentityPasswordRequiresLower);
    public const string IdentityPasswordRequiresNonAlphanumeric = nameof(IdentityPasswordRequiresNonAlphanumeric);
    public const string IdentityPasswordRequiresUniqueChars = nameof(IdentityPasswordRequiresUniqueChars);
    public const string IdentityPasswordRequiresUpper = nameof(IdentityPasswordRequiresUpper);
    public const string IdentityPasswordTooShort = nameof(IdentityPasswordTooShort);
    public const string IdentityRecoveryCodeRedemptionFailed = nameof(IdentityRecoveryCodeRedemptionFailed);
    public const string IdentityUserAlreadyHasPassword = nameof(IdentityUserAlreadyHasPassword);
    public const string IdentityUserAlreadyInRole = nameof(IdentityUserAlreadyInRole);
    public const string IdentityUserLockoutNotEnabled = nameof(IdentityUserLockoutNotEnabled);
    public const string IdentityUserNotInRole = nameof(IdentityUserNotInRole);

    #endregion

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member