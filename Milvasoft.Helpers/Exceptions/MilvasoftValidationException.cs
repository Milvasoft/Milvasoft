using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvasoftValidationException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftValidationException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public MilvasoftValidationException(IStringLocalizer localizer) : base(localizer["ValidationException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.ValidationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvasoftValidationException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.ValidationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvasoftValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvasoftValidationException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.ValidationError;
        }
    }
}
