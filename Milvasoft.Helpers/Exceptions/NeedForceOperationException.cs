using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to search for an entity not found in the Repository.
    /// </summary>
    public class NeedForceOperationException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeedForceOperationException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="localizer"></param>
        public NeedForceOperationException(IStringLocalizer localizer) : base(localizer["NeedForceOperationException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.NeedForceOperation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeedForceOperationException"/> class with an entity id not found in the Repository. 
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="customMessage"></param>
        public NeedForceOperationException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.NeedForceOperation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeedForceOperationException"/> class with an entity id not found in the Repository.
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public NeedForceOperationException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.NeedForceOperation;
        }
    }
}
