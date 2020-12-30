using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to do operation with null objects.</para>
    /// <para><b>TR: </b>Boş nesnelerle işlem yapılmaya çalışıldığında ortaya çıkan istisna.</para>
    /// </summary>
    public class InvalidLicenceException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.NullParameterException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.NullParameterException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public InvalidLicenceException(IStringLocalizer localizer) : base(localizer["InvalidLicenceException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidLicence;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.NullParameterException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.NullParameterException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public InvalidLicenceException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidLicence;
        }

        /// <summary>
        /// <para><b>EN: </b> Initializes a new instance of the <c>ABKExceptionLibrary.NullParameterException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.NullParameterException </c> sınıfının yeni bir örneğini, belirtilen bir hata iletisiyle ve bu istisnanın nedeni olan iç özel duruma bir başvuruyla nitelleştirir.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public InvalidLicenceException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.InvalidLicence;
        }
    }
}
