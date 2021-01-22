using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    //TODO kütüphaneden fırlayan hatalar milvasoftbaseexceptionlardan olacak.

    /// <summary>
    /// Base exception class.
    /// </summary>
    public abstract class MilvasoftBaseException : Exception
    {
        /// <summary>
        /// Defines error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvasoftBaseException(IStringLocalizer localizer) : base(localizer["MilvasoftBaseException"])
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvasoftExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvasoftBaseException(string customMessage) : base(customMessage)
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvasoftExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftBaseException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvasoftBaseException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvasoftExceptionCode.BaseException;
        }
    }
}
