using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with null objects.
    /// </summary>
    public class NullParameterException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullParameterException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public NullParameterException(IStringLocalizer localizer) : base(localizer["NullParameterException"])
        {
            ErrorCode = (int)MilvaExceptionCode.NullParameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullParameterException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public NullParameterException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.NullParameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullParameterException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public NullParameterException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.NullParameter;
        }
    }
}
