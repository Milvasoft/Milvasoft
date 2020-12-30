using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to add an unstockable product to Product Stock table.</para>
    /// <para><b>TR: </b>Stoklanamayan bir ürünü Ürün Stoku tablosuna eklemeye çalıştığınızda ortaya çıkan istisna.</para>
    /// </summary>
    public class CannotUpdateOrDeleteDefaultRecordException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotBeAddedProductToStockException</c> class with defined default message by constructor.</para>
        /// <para><b>TR: </b>Yapıcı tarafından tanımlanan varsayılan mesajla <c> ABKExceptionLibrary.CannotBeAddedProductToStockException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public CannotUpdateOrDeleteDefaultRecordException(IStringLocalizer localizer) : base(localizer["CannotDeleteOrUpdateDefaultRecords"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotBeAddedProductToStockException</c> class with defined default message by constructor.</para>
        /// <para><b>TR: </b>Yapıcı tarafından tanımlanan varsayılan mesajla <c> ABKExceptionLibrary.CannotBeAddedProductToStockException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotUpdateOrDeleteDefaultRecordException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotBeAddedProductToStockException</c> class with defined default message by constructor.</para>
        /// <para><b>TR: </b>Yapıcı tarafından tanımlanan varsayılan mesajla <c> ABKExceptionLibrary.CannotBeAddedProductToStockException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public CannotUpdateOrDeleteDefaultRecordException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }
    }
}
