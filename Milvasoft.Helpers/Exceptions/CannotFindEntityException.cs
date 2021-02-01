﻿using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to search for an entity not found in the Repository.
    /// </summary>
    public class CannotFindEntityException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEntityException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="localizer"></param>
        public CannotFindEntityException(IStringLocalizer localizer) : base(localizer["CannotFindEntityException"])
        {
            ErrorCode = (int)MilvaExceptionCode.CannotFindEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEntityException"/> class with an entity id not found in the Repository.
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotFindEntityException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotFindEntity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotFindEntityException"/> class with an entity id not found in the Repository.
        /// The spesific message of base class uses this invalid id when does not send a specific message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public CannotFindEntityException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotFindEntity;
        }
    }
}
