using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with null objects.
    /// </summary>
    public class InvalidLicenceException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenceException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public InvalidLicenceException(IStringLocalizer localizer) : base(localizer["InvalidLicenceException"])
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidLicence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenceException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidLicenceException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidLicence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenceException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidLicenceException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidLicence;
        }
    }
}
