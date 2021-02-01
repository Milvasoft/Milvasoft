using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to get enum by invalid id.
    /// </summary>
    public class InvalidParameterException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public InvalidParameterException(IStringLocalizer localizer) : base(localizer["InvalidParameterException"])
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidParameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="localizer"></param>
        public InvalidParameterException(IStringLocalizer localizer, string customMessage) : base(localizer["InvalidParameterException", customMessage])
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidParameter;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class with a specified error message.
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidParameterException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidParameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidParameterException(string customMessage, Exception innerException) : base("Message : " + $"{customMessage}" + " Inner Exception : " + $"{innerException}")
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidParameter;
        }
    }
}
