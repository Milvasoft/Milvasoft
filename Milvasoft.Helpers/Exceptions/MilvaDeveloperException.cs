using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvaDeveloperException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvaDeveloperException(IStringLocalizer localizer) : base(localizer[nameof(MilvaBaseException)])
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvaDeveloperException(string customMessage) : base(customMessage)
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvaDeveloperException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ExceptionCode = (int)MilvaExceptionCode.BaseException;
        }
    }
}
