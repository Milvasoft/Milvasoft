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
        public int ExceptionCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvaBaseException(IStringLocalizer localizer) : base(localizer[nameof(MilvaBaseException)])
        {
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvaBaseException(string customMessage) : base(customMessage)
        {
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvaBaseException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaExceptionCode.BaseException;
        }
    }
}
