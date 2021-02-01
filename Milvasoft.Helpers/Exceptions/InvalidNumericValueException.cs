using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to process invalid numeric value in the conditions of situation.
    /// </summary>
    public class InvalidNumericValueException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidNumericValueException"/> class.
        /// </summary>
        /// <param name="localizer"></param>
        public InvalidNumericValueException(IStringLocalizer localizer) : base(localizer["InvalidNumericValueException"])
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidNumericValueException"/> class  with invalid value. 
        /// The spesific message of base class uses this invalid value when does not send a specific message.
        /// </summary>
        /// <param name="invalidValue"></param>
        /// <param name="localizer"></param>
        public InvalidNumericValueException(IStringLocalizer localizer, decimal invalidValue) : base(localizer["InvalidNumericValueException", invalidValue.ToString("{0:0.##}")])
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidNumericValueException"/> class  with a specified error message.
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidNumericValueException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidNumericValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidNumericValueException"/> class  with invalid value. 
        /// The spesific message of base class uses this invalid value when does not send a specific message.
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidNumericValueException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvaExceptionCode.InvalidNumericValue;
        }
    }
}
