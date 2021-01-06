using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do process on empty list.
    /// </summary>
    public class EmptyListException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyListException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="localizer"></param>
        public EmptyListException(IStringLocalizer localizer) : base(localizer["EmptyListException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyListException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public EmptyListException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyListException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public EmptyListException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }
    }
}
