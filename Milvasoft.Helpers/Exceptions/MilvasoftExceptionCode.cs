namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>Defines the OpsiyonErrorCode.</para>
    /// <para><b>TR: </b>OpsiyonErrorCode'u tanımlar.</para>
    /// </summary>
    public enum MilvasoftExceptionCode
    {
        /// <summary>
        /// <para><b>EN: </b> Defines the UnknownError.</para>
        /// <para><b>TR: </b>UnknownError'yi tanımlar.</para>
        /// </summary>
        UnknownError = 0,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotFindEntity.</para>
        /// <para><b>TR: </b>CannotFindEntity'yi tanımlar.</para>
        /// </summary>
        CannotFindEntity = 1,

        /// <summary>
        /// <para><b>EN: </b> Defines the AddingNewEntityWithExsistId.</para>
        /// <para><b>TR: </b>AddingNewEntityWithExsistId'yi tanımlar.</para>
        /// </summary>
        AddingNewEntityWithExsistId = 2,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotBeAddedProduct.</para>
        /// <para><b>TR: </b>CannotBeAddedProduct'yi tanımlar.</para>
        /// </summary>
        CannotBeAddedProductToStock = 3,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotAddTheEntitiesToAllOfRepositories.</para>
        /// <para><b>TR: </b>CannotAddTheEntitiesToAllOfRepositories'yi tanımlar.</para>
        /// </summary>
        CannotAddTheEntitiesToAllOfRepositories = 4,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotFindEnumById.</para>
        /// <para><b>TR: </b>CannotFindEnumById'yi tanımlar.</para>
        /// </summary>
        CannotFindEnumById = 5,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotStartRedisServer.</para>
        /// <para><b>TR: </b>CannotStartRedisServer'yi tanımlar.</para>
        /// </summary>
        CannotStartRedisServer = 6,

        /// <summary>
        /// <para><b>EN: </b> Defines the DeletingEntityWithRelations.</para>
        /// <para><b>TR: </b>DeletingEntityWithRelations'yi tanımlar.</para>
        /// </summary>
        DeletingEntityWithRelations = 7,

        /// <summary>
        /// <para><b>EN: </b> Defines the DeletingInvalidEntity.</para>
        /// <para><b>TR: </b>DeletingInvalidEntity'yi tanımlar.</para>
        /// </summary>
        DeletingInvalidEntity = 8,

        /// <summary>
        /// <para><b>EN: </b> Defines the DeletingSupplierWithDebt.</para>
        /// <para><b>TR: </b>DeletingSupplierWithDebt'yi tanımlar.</para>
        /// </summary>
        DeletingSupplierWithDebt = 9,

        /// <summary>
        /// <para><b>EN: </b> Defines the EmptyList.</para>
        /// <para><b>TR: </b>EmptyList'yi tanımlar.</para>
        /// </summary>
        EmptyList = 10,

        /// <summary>
        /// <para><b>EN: </b> Defines the InvalidNumericValue.</para>
        /// <para><b>TR: </b>InvalidNumericValue'yi tanımlar.</para>
        /// </summary>
        InvalidNumericValue = 11,

        /// <summary>
        /// <para><b>EN: </b> Defines the InvalidParameter.</para>
        /// <para><b>TR: </b>InvalidParameter'yi tanımlar.</para>
        /// </summary>
        InvalidParameter = 12,

        /// <summary>
        /// <para><b>EN: </b> Defines the InvalidStringExpression.</para>
        /// <para><b>TR: </b>InvalidStringExpression'yi tanımlar.</para>
        /// </summary>
        InvalidStringExpression = 13,

        /// <summary>
        /// <para><b>EN: </b> Defines the NullParameter.</para>
        /// <para><b>TR: </b>NullParameter'yi tanımlar.</para>
        /// </summary>
        NullParameter = 14,

        /// <summary>
        /// <para><b>EN: </b> Defines the UnknownStockType.</para>
        /// <para><b>TR: </b>UnknownStockType'yi tanımlar.</para>
        /// </summary>
        UnknownStockType = 15,

        /// <summary>
        /// <para><b>EN: </b> Defines the UpdatingInvalidEntity.</para>
        /// <para><b>TR: </b>UpdatingInvalidEntity'yi tanımlar.</para>
        /// </summary>
        UpdatingInvalidEntity = 16,

        /// <summary>
        /// Defines the NotLoggedInUserException.
        /// </summary>
        NotLoggedInUser = 17,

        /// <summary>
        /// Defines the NotLoggedInUserException.
        /// </summary>
        AnotherLoginExists = 18,

        /// <summary>
        /// <para><b>EN: </b> Defines the UpdatingInvalidEntity.</para>
        /// <para><b>TR: </b>UpdatingInvalidEntity'yi tanımlar.</para>
        /// </summary>
        OldVersion = 19,

        /// <summary>
        /// <para><b>EN: </b> Defines the UpdatingInvalidEntity.</para>
        /// <para><b>TR: </b>UpdatingInvalidEntity'yi tanımlar.</para>
        /// </summary>
        InvalidLicence = 20,

        /// <summary>
        /// <para><b>EN: </b> Defines the UpdatingInvalidEntity.</para>
        /// <para><b>TR: </b>UpdatingInvalidEntity'yi tanımlar.</para>
        /// </summary>
        ExpiredDemo = 21,

        /// <summary>
        /// <para><b>EN: </b>Defines the NeedForceOperation.</para>
        /// <para><b>TR: </b>NeedForceOperation'yi tanımlar.</para>
        /// </summary>
        NeedForceOperation = 22,

        /// <summary>
        /// <para><b>EN: </b>Defines the NeedForceOperation.</para>
        /// <para><b>TR: </b>NeedForceOperation'yi tanımlar.</para>
        /// </summary>
        OutOfStock = 23,

        /// <summary>
        /// <para><b>EN: </b>Defines the NeedForceOperation.</para>
        /// <para><b>TR: </b>NeedForceOperation'yi tanımlar.</para>
        /// </summary>
        InvalidMateralDeficit = 24,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotUpdateOrDeleteDefaultRecord.</para>
        /// <para><b>TR: </b> CannotUpdateOrDeleteDefaultRecord'yi tanımlar.</para>
        /// </summary>
        CannotUpdateOrDeleteDefaultRecord = 25,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotUpdateOrDeleteDefaultRecord.</para>
        /// <para><b>TR: </b> CannotUpdateOrDeleteDefaultRecord'yi tanımlar.</para>
        /// </summary>
        ValidationError = 26,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotUpdateOrDeleteDefaultRecord.</para>
        /// <para><b>TR: </b> CannotUpdateOrDeleteDefaultRecord'yi tanımlar.</para>
        /// </summary>
        GeneralException = 27,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotUpdateOrDeleteDefaultRecord.</para>
        /// <para><b>TR: </b> CannotUpdateOrDeleteDefaultRecord'yi tanımlar.</para>
        /// </summary>
        InvalidRelatedProcess = 28,

        /// <summary>
        /// <para><b>EN: </b> Defines the CannotUpdateOrDeleteDefaultRecord.</para>
        /// <para><b>TR: </b> CannotUpdateOrDeleteDefaultRecord'yi tanımlar.</para>
        /// </summary>
        FeatureNotImplementedException = 29

    }
}
