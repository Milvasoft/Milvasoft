using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to update an entity that is not in the Repository.
    /// </summary>
    public class UpdatingInvalidEntityException : MilvaBaseException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatingInvalidEntityException"/> class with an entity id not found in the Repository. 
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="localizer"></param>
        public UpdatingInvalidEntityException(IStringLocalizer localizer) : base(localizer["UpdatingInvalidEntityException"])
        {
            ErrorCode = (int)MilvaExceptionCode.UpdatingInvalidEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatingInvalidEntityException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public UpdatingInvalidEntityException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.UpdatingInvalidEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatingInvalidEntityException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public UpdatingInvalidEntityException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.UpdatingInvalidEntity;
        }

    }
}
