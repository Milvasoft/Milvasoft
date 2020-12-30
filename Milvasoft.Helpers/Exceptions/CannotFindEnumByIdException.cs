using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to get enum by invalid id.</para>
    /// <para><b>TR: </b>Geçersiz id ile numaralandırma girişiminde bulunulduğunda ortaya çıkan istisna.</para>
    /// </summary>
    public class CannotFindEnumByIdException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEnumByIdException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.CannotFindEnumByIdException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public CannotFindEnumByIdException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEnumByIdException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.CannotFindEnumByIdException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotFindEnumByIdException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEnumByIdException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.CannotFindEnumByIdException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>s
        /// <param name="innerException"></param>
        public CannotFindEnumByIdException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }
    }
}
