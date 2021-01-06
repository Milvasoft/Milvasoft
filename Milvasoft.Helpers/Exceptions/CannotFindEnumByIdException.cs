using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to get enum by invalid id.
    /// </summary>
    public class CannotFindEnumByIdException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public CannotFindEnumByIdException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotFindEnumByIdException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>s
        /// <param name="innerException"></param>
        public CannotFindEnumByIdException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEnumById;
        }
    }
}
