using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with not implemented method.
    /// </summary>
    public class FeatureNotImplementedException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNotImplementedException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public FeatureNotImplementedException(IStringLocalizer localizer) : base(localizer["FeatureNotImplementedException"])
        {
            ErrorCode = (int)MilvaExceptionCode.FeatureNotImplemented;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNotImplementedException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public FeatureNotImplementedException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.FeatureNotImplemented;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNotImplementedException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="exception"></param>
        public FeatureNotImplementedException(string customMessage, Exception exception) : base(customMessage, exception)
        {
            ErrorCode = (int)MilvaExceptionCode.FeatureNotImplemented;
        }
    }
}
