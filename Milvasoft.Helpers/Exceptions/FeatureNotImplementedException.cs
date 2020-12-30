using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>Exception to be thrown for out of stock items</para>
    /// <para><b>TR: </b>Stokda kalmayan ürünler için fırlatılacak olan istisna</para>
    /// </summary>
    public class FeatureNotImplementedException : MilvasoftBaseException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringLocalizer"></param>
        public FeatureNotImplementedException(IStringLocalizer stringLocalizer) : base(stringLocalizer["FeatureNotImplementedException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.FeatureNotImplementedException;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public FeatureNotImplementedException(string message) : base(message)
        {
            ErrorCode = (int)MilvasoftExceptionCode.FeatureNotImplementedException;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public FeatureNotImplementedException(string message, Exception exception):base(message, exception)
        {
            ErrorCode = (int)MilvasoftExceptionCode.FeatureNotImplementedException;
        }
    }
}
