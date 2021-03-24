namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// Defines the Milvasoft Exception Codes.
    /// </summary>
    public enum MilvaException
    {
        /// <summary>
        /// Defines the UnknownError.
        /// </summary>
        Base = 0,

        /// <summary>
        /// Defines the CannotFindEntity.
        /// </summary>
        General = 1,

        /// <summary>
        /// Defines the CannotFindEntity.
        /// </summary>
        CannotFindEntity = 2,

        /// <summary>
        /// Defines the AddingNewEntityWithExsistId.
        /// </summary>
        AddingNewEntityWithExistsId = 3,

        /// <summary>
        /// Defines the EncryptionError.
        /// </summary>
        Encryption = 4,

        /// <summary>
        /// Defines the CannotFindEnumById.
        /// </summary>
        CannotFindEnumById = 5,

        /// <summary>
        /// Defines the CannotStartRedisServer.
        /// </summary>
        CannotStartRedisServer = 6,

        /// <summary>
        /// Defines the DeletingEntityWithRelations.
        /// </summary>
        DeletingEntityWithRelations = 7,

        /// <summary>
        /// Defines the DeletingInvalidEntity.
        /// </summary>
        DeletingInvalidEntity = 8,

        /// <summary>
        /// Defines the TenancyNameRequired.
        /// </summary>
        TenancyNameRequired = 9,

        /// <summary>
        /// Defines the EmptyList.
        /// </summary>
        EmptyList = 10,

        /// <summary>
        /// Defines the InvalidNumericValue.
        /// </summary>
        InvalidNumericValue = 11,

        /// <summary>
        /// Defines the InvalidParameter.
        /// </summary>
        InvalidParameter = 12,

        /// <summary>
        /// Defines the InvalidStringExpression.
        /// </summary>
        InvalidStringExpression = 13,

        /// <summary>
        /// Defines the NullParameter.
        /// </summary>
        NullParameter = 14,

        /// <summary>
        /// Defines the FeatureNotImplemented.
        /// </summary>
        FeatureNotImplemented = 15,

        /// <summary>
        /// Defines the UpdatingInvalidEntity.
        /// </summary>
        UpdatingInvalidEntity = 16,

        /// <summary>
        /// Defines the NotLoggedInUser.
        /// </summary>
        NotLoggedInUser = 17,

        /// <summary>
        /// Defines the AnotherLoginExists.
        /// </summary>
        AnotherLoginExists = 18,

        /// <summary>
        /// Defines the OldVersion.
        /// </summary>
        OldVersion = 19,

        /// <summary>
        /// Defines the InvalidLicence.
        /// </summary>
        InvalidLicence = 20,

        /// <summary>
        /// Defines the ExpiredDemo.
        /// </summary>
        ExpiredDemo = 21,

        /// <summary>
        /// Defines the NeedForceOperation.
        /// </summary>
        NeedForceOperation = 22,

        /// <summary>
        /// Defines the CannotGetResponse.
        /// </summary>
        CannotGetResponse = 23,

        /// <summary>
        /// Defines the IdentityResult.
        /// </summary>
        IdentityResult = 24,

        /// <summary>
        /// Defines the CannotUpdateOrDeleteDefaultRecord.
        /// </summary>
        CannotUpdateOrDeleteDefaultRecord = 25,

        /// <summary>
        /// Defines the ValidationError.
        /// </summary>
        Validation = 26,

        /// <summary>
        /// Defines the InvalidRelatedProcess.
        /// </summary>
        InvalidRelatedProcess = 27,

        /// <summary>
        /// Defines the WrongPaginationParams.
        /// </summary>
        WrongPaginationParams = 28,

        /// <summary>
        /// Defines the WrongPaginationParams.
        /// </summary>
        WrongRequestedPageNumber = 29,

        /// <summary>
        /// Defines the WrongPaginationParams.
        /// </summary>
        WrongRequestedItemCount = 30,
    }
}
