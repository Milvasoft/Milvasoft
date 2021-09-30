namespace Milvasoft.Helpers.Utils
{
    /// <summary>
    /// Localizer message keys
    /// </summary>
    public static class LocalizerKeys
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string DefaultErrorMessage = "An error occured while processing";
        public const string LocalizedEntityName = nameof(LocalizedEntityName);
        public const string Localized = nameof(Localized);

        #region Response Message Keys

        public const string SuccessfullyOperationMessage = nameof(SuccessfullyOperationMessage);
        public const string AddErrorMessage = nameof(AddErrorMessage);
        public const string UpdateErrorMessage = nameof(UpdateErrorMessage);
        public const string DeleteErrorMessage = nameof(DeleteErrorMessage);
        public const string GetByIdErrorMessage = nameof(GetByIdErrorMessage);
        public const string GetAllErrorMessage = nameof(GetAllErrorMessage);
        public const string FilteringErrorMessage = nameof(FilteringErrorMessage);
        public const string AddSuccessMessage = nameof(AddSuccessMessage);
        public const string UpdateSuccessMessage = nameof(UpdateSuccessMessage);
        public const string DeleteSuccessMessage = nameof(DeleteSuccessMessage);
        public const string GetByIdSuccessMessage = nameof(GetByIdSuccessMessage);
        public const string GetAllSuccessMessage = nameof(GetAllSuccessMessage);
        public const string FilteringSuccessMessage = nameof(FilteringSuccessMessage);

        #endregion

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

        #region Attributes

        public const string PleaseEnterAValid = "Please enter a valid";
        public const string PreventStringInjectionLengthResultNotTrue = nameof(PreventStringInjectionLengthResultNotTrue);
        public const string PreventStringInjectionContainsForbiddenWordError = nameof(PreventStringInjectionContainsForbiddenWordError);
        public const string PreventStringInjectionBellowMin = nameof(PreventStringInjectionBellowMin);
        public const string PreventStringInjectionMaxLengthException = nameof(PreventStringInjectionMaxLengthException);
        public const string PreventStringInjectionMinLengthException = nameof(PreventStringInjectionMinLengthException);
        public const string PreventStringInjectionMinLengthBigThanMaxLengthException = nameof(PreventStringInjectionMinLengthBigThanMaxLengthException);
        public const string MinDecimalValueException = nameof(MinDecimalValueException);
        public const string ValidationIdPropertyError = nameof(ValidationIdPropertyError);
        public const string ValidationIdParameterGeneralError = nameof(ValidationIdParameterGeneralError);
        public const string RegexErrorMessage = nameof(RegexErrorMessage);
        public const string RegexExample = nameof(RegexExample);
        public const string RegexPattern = nameof(RegexPattern);
        public const string PropertyIsRequired = nameof(PropertyIsRequired);
        public const string ListMaxCountMessage = nameof(ListMaxCountMessage);
        public const string ListMinCountMessage = nameof(ListMinCountMessage);
        public const string ListCountNotValid = nameof(ListCountNotValid);
        public const string ListCountBelowMin = nameof(ListCountBelowMin);
        public const string ListCountMustBe = nameof(ListCountMustBe);

        #endregion

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

        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    }
}
