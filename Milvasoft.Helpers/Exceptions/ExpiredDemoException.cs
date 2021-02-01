using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with null objects.
    /// </summary>
    public class ExpiredDemoException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredDemoException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public ExpiredDemoException(IStringLocalizer localizer) : base(localizer["ExpiredDemoException"])
        {
            ErrorCode = (int)MilvaExceptionCode.ExpiredDemo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredDemoException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public ExpiredDemoException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.ExpiredDemo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredDemoException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public ExpiredDemoException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.ExpiredDemo;
        }
    }
}
