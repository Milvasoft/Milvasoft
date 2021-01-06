using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with null objects.
    /// </summary>
    public class InvalidRelatedProcessException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRelatedProcessException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public InvalidRelatedProcessException(IStringLocalizer localizer) : base(localizer["InvalidRelatedProcess"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidRelatedProcess;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRelatedProcessException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidRelatedProcessException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidRelatedProcess;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRelatedProcessException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidRelatedProcessException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidRelatedProcess;
        }
    }
}
