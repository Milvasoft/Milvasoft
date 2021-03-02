namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// Defines the MilvasoftExceptionCode.
    /// </summary>
    public enum MilvaExceptionCode : int
    {
        /// <summary>
        /// Defines the UnknownError.
        /// </summary>
        BaseException = 0,

        /// <summary>
        /// Defines the CannotFindEntity.
        /// </summary>
        CannotFindEntityException = 1,

        /// <summary>
        /// Defines the AddingNewEntityWithExsistId.
        /// </summary>
        AddingNewEntityWithExistsIdException = 2,

        /// <summary>
        /// Defines the CannotBeAddedProductToStock.
        /// </summary>
        CannotBeAddedProductToStockException = 3,

        /// <summary>
        /// Defines the CannotAddTheEntitiesToAllOfRepositories.
        /// </summary>
        CannotAddTheEntitiesToAllOfRepositoriesException = 4,

        /// <summary>
        /// Defines the CannotFindEnumById.
        /// </summary>
        CannotFindEnumByIdExceptionException = 5,

        /// <summary>
        /// Defines the CannotStartRedisServer.
        /// </summary>
        CannotStartRedisServerException = 6,

        /// <summary>
        /// Defines the DeletingEntityWithRelations.
        /// </summary>
        DeletingEntityWithRelationsException = 7,

        /// <summary>
        /// Defines the DeletingInvalidEntity.
        /// </summary>
        DeletingInvalidEntityException = 8,

        /// <summary>
        /// Defines the DeletingSupplierWithDebt.
        /// </summary>
        DeletingSupplierWithDebtException = 9,

        /// <summary>
        /// Defines the EmptyList.
        /// </summary>
        EmptyListException = 10,

        /// <summary>
        /// Defines the InvalidNumericValue.
        /// </summary>
        InvalidNumericValueException = 11,

        /// <summary>
        /// Defines the InvalidParameter.
        /// </summary>
        InvalidParameterException = 12,

        /// <summary>
        /// Defines the InvalidStringExpression.
        /// </summary>
        InvalidStringExpressionException = 13,

        /// <summary>
        /// Defines the NullParameter.
        /// </summary>
        NullParameterException = 14,

        /// <summary>
        /// Defines the UnknownStockType.
        /// </summary>
        UnknownStockTypeException = 15,

        /// <summary>
        /// Defines the UpdatingInvalidEntity.
        /// </summary>
        UpdatingInvalidEntityException = 16,

        /// <summary>
        /// Defines the NotLoggedInUser.
        /// </summary>
        NotLoggedInUserException = 17,

        /// <summary>
        /// Defines the AnotherLoginExists.
        /// </summary>
        AnotherLoginExistsException = 18,

        /// <summary>
        /// Defines the OldVersion.
        /// </summary>
        OldVersionException = 19,

        /// <summary>
        /// Defines the InvalidLicence.
        /// </summary>
        InvalidLicenceException = 20,

        /// <summary>
        /// Defines the ExpiredDemo.
        /// </summary>
        ExpiredDemoException = 21,

        /// <summary>
        /// Defines the NeedForceOperation.
        /// </summary>
        NeedForceOperationException = 22,

        /// <summary>
        /// Defines the OutOfStock.
        /// </summary>
        OutOfStockException = 23,

        /// <summary>
        /// Defines the InvalidMateralDeficit.
        /// </summary>
        InvalidMateralDeficitException = 24,

        /// <summary>
        /// Defines the CannotUpdateOrDeleteDefaultRecord.
        /// </summary>
        CannotUpdateOrDeleteDefaultRecordException = 25,

        /// <summary>
        /// Defines the ValidationError.
        /// </summary>
        ValidationErrorException = 26,

        /// <summary>
        /// Defines the GeneralException.
        /// </summary>
        GeneralException = 27,

        /// <summary>
        /// Defines the InvalidRelatedProcess.
        /// </summary>
        InvalidRelatedProcessException = 28,

        /// <summary>
        /// Defines the FeatureNotImplementedException.
        /// </summary>
        FeatureNotImplementedException = 29,

        /// <summary>
        /// Defines the IdentityResultException.
        /// </summary>
        IdentityResultException = 30,

        /// <summary>
        /// Defines the CannotGetResponseException.
        /// </summary>
        CannotGetResponseException = 31,

        /// <summary>
        /// Defines the EncryptionErrorException.
        /// </summary>
        EncryptionErrorException = 32,

        /// <summary>
        /// Defines the WrongPaginationParamsException.
        /// </summary>
        WrongPaginationParamsException = 33,

        /// <summary>
        /// Defines the WrongPaginationParamsException.
        /// </summary>
        WrongRequestedPageNumberException = 34,

        /// <summary>
        /// Defines the WrongPaginationParamsException.
        /// </summary>
        WrongRequestedItemCountException = 35
    }
}
