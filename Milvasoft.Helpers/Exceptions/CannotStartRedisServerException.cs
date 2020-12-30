using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>Exception class to be thrown when Redis Server fails to start</para>
    /// <para><b>TR: </b>Redis Sunucu başlatılamadığı durumlarda fırlatılacak istisna sınıfı</para>
    /// </summary>
    public class CannotStartRedisServerException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotStartRedisServer</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.CannotStartRedisServer </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public CannotStartRedisServerException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotStartRedisServer;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotStartRedisServer</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.CannotStartRedisServer </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotStartRedisServerException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotStartRedisServer;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotStartRedisServer</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.CannotStartRedisServer </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CannotStartRedisServerException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotStartRedisServer;
        }
    }
}
