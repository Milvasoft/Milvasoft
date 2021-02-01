using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to do operation with null objects.
    /// </summary>
    public class OldVersionException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OldVersionException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public OldVersionException(IStringLocalizer localizer) : base(localizer["OldVersionException"])
        {
            ErrorCode = (int)MilvaExceptionCode.OldVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OldVersionException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public OldVersionException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.OldVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OldVersionException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public OldVersionException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.OldVersion;
        }
    }
}
