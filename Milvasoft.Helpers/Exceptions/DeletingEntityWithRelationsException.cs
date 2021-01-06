using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to deleting invalid entities.
    /// </summary>
    public class DeletingEntityWithRelationsException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingEntityWithRelationsException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public DeletingEntityWithRelationsException(IStringLocalizer localizer) : base(localizer["DeletingEntityWithRelationsException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingEntityWithRelationsException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public DeletingEntityWithRelationsException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingEntityWithRelationsException"/> class.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public DeletingEntityWithRelationsException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }
    }
}
