using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to do operations with unsupported string by process.</para>
    /// <para><b>TR: </b>İşlem bazında desteklenmeyen dizge ile işlemler yapılmaya çalışıldığında ortaya çıkan istisna.</para>
    /// </summary>
    public class InvalidStringExpressionException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidStringExpressionException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.InvalidStringExpressionException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public InvalidStringExpressionException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidStringExpression;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidStringExpressionException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.InvalidStringExpressionException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidStringExpressionException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidStringExpression;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidStringExpressionException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata iletisiyle <c> ABKExceptionLibrary.InvalidStringExpressionException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidStringExpressionException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidStringExpression;
        }
    }
}
