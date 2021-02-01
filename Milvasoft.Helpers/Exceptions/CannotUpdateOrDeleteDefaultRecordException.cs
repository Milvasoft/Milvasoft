using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an unstockable product to Product Stock table.
    /// </summary>
    public class CannotUpdateOrDeleteDefaultRecordException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CannotUpdateOrDeleteDefaultRecordException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="localizer"></param>
        public CannotUpdateOrDeleteDefaultRecordException(IStringLocalizer localizer) : base(localizer["CannotDeleteOrUpdateDefaultRecords"])
        {
            ErrorCode = (int)MilvaExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotUpdateOrDeleteDefaultRecordException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotUpdateOrDeleteDefaultRecordException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotUpdateOrDeleteDefaultRecordException"/> class with defined default message by constructor.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public CannotUpdateOrDeleteDefaultRecordException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.CannotUpdateOrDeleteDefaultRecord;
        }
    }
}
