using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvaValidationException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvaValidationException(IStringLocalizer localizer) : base(localizer[nameof(MilvaValidationException)])
        {
            ExceptionCode = MilvaExceptionCode.ValidationErrorException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvaValidationException(string customMessage) : base(customMessage)
        {
            ExceptionCode = MilvaExceptionCode.ValidationErrorException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvaValidationException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ExceptionCode = MilvaExceptionCode.ValidationErrorException;
        }
    }
}
