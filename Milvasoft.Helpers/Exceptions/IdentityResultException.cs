using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when identity exceptions.
    /// </summary>
    public class IdentityResultException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResultException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public IdentityResultException(IStringLocalizer localizer) : base(localizer["InvalidParameterException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.IdentityResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResultException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="localizer"></param>
        public IdentityResultException(IStringLocalizer localizer, string customMessage) : base(localizer["InvalidParameterException", customMessage])
        {
            ErrorCode = (int)MilvasoftExceptionCode.IdentityResult;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResultException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public IdentityResultException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.IdentityResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResultException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public IdentityResultException(string customMessage, Exception innerException) : base("Message : " + $"{customMessage}" + " Inner Exception : " + $"{innerException}")
        {
            ErrorCode = (int)MilvasoftExceptionCode.IdentityResult;
        }
    }
}
