using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class AddingNewEntityWithExistsIdException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddingNewEntityWithExistsIdException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public AddingNewEntityWithExistsIdException(IStringLocalizer localizer) : base(localizer["AddingNewEntityWithExistsIdException"])
        {
            ErrorCode = (int)MilvaExceptionCode.AddingNewEntityWithExsistId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddingNewEntityWithExistsIdException"/> class with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public AddingNewEntityWithExistsIdException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.AddingNewEntityWithExsistId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddingNewEntityWithExistsIdException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public AddingNewEntityWithExistsIdException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.AddingNewEntityWithExsistId;
        }
    }
}
