using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to get enum by invalid id.
    /// </summary>
    public class CannotGetResponseException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public CannotGetResponseException(IStringLocalizer localizer) : base(localizer)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotGetResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotGetResponseException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotGetResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEnumByIdException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>s
        /// <param name="innerException"></param>
        public CannotGetResponseException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotGetResponse;
        }
    }
}
