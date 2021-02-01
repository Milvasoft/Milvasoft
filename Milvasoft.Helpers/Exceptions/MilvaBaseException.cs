using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// Base exception class.
    /// </summary>
    public abstract class MilvaBaseException : Exception
    {
        /// <summary>
        /// Defines error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvaBaseException(IStringLocalizer localizer) : base(localizer["MilvasoftBaseException"])
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvaBaseException(string customMessage) : base(customMessage)
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvaBaseException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = ErrorCode != 0 ? ErrorCode : (int)MilvaExceptionCode.BaseException;
        }
    }
}
