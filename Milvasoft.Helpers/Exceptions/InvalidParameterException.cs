using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to get enum by invalid id.</para>
    /// <para><b>TR: </b>Geçersiz id ile numaralandırma girişiminde bulunulduğunda ortaya çıkan istisna.</para>
    /// </summary>
    public class InvalidParameterException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>InvalidParameter.CannotStartRedisServer</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.InvalidParameter </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public InvalidParameterException(IStringLocalizer localizer) : base(localizer["InvalidParameterException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidParameter;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidParameter</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.InvalidParameter </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="localizer"></param>
        public InvalidParameterException(IStringLocalizer localizer, string message) : base(localizer["InvalidParameterException", message])
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidParameter;
        }


        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.AddingNewEntityWithExistsIdException</c> class with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajı ile <c> ABKExceptionLibrary.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidParameterException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidParameter;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.InvalidParameter</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.InvalidParameter </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InvalidParameterException(string message, Exception innerException) : base("Message : " + $"{message}" + " Inner Exception : " + $"{innerException}")
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidParameter;
        }
    }
}
