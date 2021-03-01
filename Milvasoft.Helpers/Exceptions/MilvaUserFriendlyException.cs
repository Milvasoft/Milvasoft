using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvaUserFriendlyException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvaUserFriendlyException(IStringLocalizer localizer) : base(localizer[nameof(MilvaUserFriendlyException)])
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvaUserFriendlyException(string customMessage) : base(customMessage)
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvaUserFriendlyException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="milvaExceptionCode"></param>
        public MilvaUserFriendlyException(IStringLocalizer localizer, MilvaExceptionCode milvaExceptionCode) : base(localizer[milvaExceptionCode.ToString()])
        {
            ExceptionCode = (int)milvaExceptionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="milvaExceptionCode"></param>
        public MilvaUserFriendlyException(string customMessage, MilvaExceptionCode milvaExceptionCode) : base(customMessage)
        {
            ExceptionCode = (int)milvaExceptionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="milvaExceptionCode"></param>
        /// <param name="innerException"></param>
        public MilvaUserFriendlyException(string customMessage, MilvaExceptionCode milvaExceptionCode, Exception innerException) : base(customMessage, innerException)
        {
            ExceptionCode = (int)milvaExceptionCode;
        }
    }
}
