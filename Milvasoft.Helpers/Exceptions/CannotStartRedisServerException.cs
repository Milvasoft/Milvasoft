using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// Exception class to be thrown when Redis Server fails to start.
    /// </summary>
    public class CannotStartRedisServerException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CannotStartRedisServerException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public CannotStartRedisServerException(IStringLocalizer localizer) : base(localizer["CannotStartRedisServerException"])
        {
            ErrorCode = (int)MilvaExceptionCode.CannotStartRedisServer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotStartRedisServerException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotStartRedisServerException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotStartRedisServer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotStartRedisServerException"/> class  with a specified error message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CannotStartRedisServerException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotStartRedisServer;
        }
    }
}
