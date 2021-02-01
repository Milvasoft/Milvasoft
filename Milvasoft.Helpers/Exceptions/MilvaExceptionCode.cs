namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// Defines the MilvasoftExceptionCode.
    /// </summary>
    public enum MilvaExceptionCode
    {
        /// <summary>
        /// Defines the UnknownError.
        /// </summary>
        BaseException = 0,

        /// <summary>
        /// Defines the CannotFindEntity.
        /// </summary>
        CannotFindEntity = 1,

        /// <summary>
        /// Defines the AddingNewEntityWithExsistId.
        /// </summary>
        AddingNewEntityWithExsistId = 2,

        /// <summary>
        /// Defines the CannotBeAddedProductToStock.
        /// </summary>
        CannotBeAddedProductToStock = 3,

        /// <summary>
        /// Defines the CannotAddTheEntitiesToAllOfRepositories.
        /// </summary>
        CannotAddTheEntitiesToAllOfRepositories = 4,

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
        /// Defines the DeletingSupplierWithDebt.
        /// </summary>
        DeletingSupplierWithDebt = 9,

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
        /// Defines the UnknownStockType.
        /// </summary>
        UnknownStockType = 15,

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
        /// Defines the OutOfStock.
        /// </summary>
        OutOfStock = 23,

        /// <summary>
        /// Defines the InvalidMateralDeficit.
        /// </summary>
        InvalidMateralDeficit = 24,

        /// <summary>
        /// Defines the CannotUpdateOrDeleteDefaultRecord.
        /// </summary>
        CannotUpdateOrDeleteDefaultRecord = 25,

        /// <summary>
        /// Defines the ValidationError.
        /// </summary>
        ValidationError = 26,

        /// <summary>
        /// Defines the GeneralException.
        /// </summary>
        GeneralException = 27,

        /// <summary>
        /// Defines the InvalidRelatedProcess.
        /// </summary>
        InvalidRelatedProcess = 28,

        /// <summary>
        /// Defines the FeatureNotImplementedException.
        /// </summary>
        FeatureNotImplemented = 29,

        /// <summary>
        /// Defines the IdentityResultException.
        /// </summary>
        IdentityResult = 30,

        /// <summary>
        /// Defines the IdentityResultException.
        /// </summary>
        CannotGetResponse = 31

    }
}
