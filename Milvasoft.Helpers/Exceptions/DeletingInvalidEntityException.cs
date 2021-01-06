using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to delete an entity that is not in the Repository.
    /// </summary>
    public class DeletingInvalidEntityException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <c>ABKExceptionLibrary.DeletingInvalidEntityException</c> class  with an entity id not found in the Repository. 
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="localizer"></param>
        public DeletingInvalidEntityException(IStringLocalizer localizer) : base(localizer["DeletingInvalidEntityException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingInvalidEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingEntityWithRelationsException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public DeletingInvalidEntityException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingInvalidEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingEntityWithRelationsException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public DeletingInvalidEntityException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingInvalidEntity;
        }
    }
}
