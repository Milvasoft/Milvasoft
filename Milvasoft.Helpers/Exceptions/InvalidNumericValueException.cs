using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to process invalid numeric value in the conditions of situation.</para>
    /// <para><b>TR: </b>Durum koşullarında geçersiz sayısal değeri işlemeye çalışırken ortaya çıkan istisna.</para>
    /// </summary>
    public class InvalidNumericValueException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidNumericValueException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.InvalidNumericValueException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        private InvalidNumericValueException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidNumericValueException</c> class  with invalid value. The spesific message of base class uses this invalid value when does not send a specific message.</para>
        /// <para><b>TR: </b>Geçersiz değere sahip <c> ABKExceptionLibrary.InvalidNumericValueException </c> sınıfının yeni bir örneğini başlatır. Temel sınıfın özel mesajı, belirli bir mesaj göndermediğinde bu geçersiz değeri kullanır.</para>
        /// </summary>
        /// <param name="invalidValue"></param>
        /// <param name="localizer"></param>
        public InvalidNumericValueException(IStringLocalizer localizer, decimal invalidValue) : base(localizer["InvalidNumericValueException", invalidValue.ToString("{0:0.##}")])
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidNumericValueException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.InvalidNumericValueException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidNumericValueException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidNumericValueException</c> class  with invalid value. The spesific message of base class uses this invalid value when does not send a specific message.</para>
        /// <para><b>TR: </b>Geçersiz değere sahip <c> ABKExceptionLibrary.InvalidNumericValueException </c> sınıfının yeni bir örneğini başlatır. Temel sınıfın özel mesajı, belirli bir mesaj göndermediğinde bu geçersiz değeri kullanır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidNumericValueException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidNumericValue;
        }
    }
}
