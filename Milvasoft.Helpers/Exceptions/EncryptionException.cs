using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when any encryption error.
    /// </summary>
    public class EncryptionException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public EncryptionException(IStringLocalizer localizer) : base(localizer["ValidationException"])
        {
            ErrorCode = (int)MilvaExceptionCode.EncryptionError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public EncryptionException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.EncryptionError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public EncryptionException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.EncryptionError;
        }
    }
}
