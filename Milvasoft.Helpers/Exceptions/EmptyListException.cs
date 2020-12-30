using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b> The exception that is thrown when attempt to do process on empty list.</para>
    /// <para><b>TR: </b>Boş listede işlem yapılmaya çalışıldığında ortaya çıkan istisna.</para>
    /// </summary>
    public class EmptyListException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.EmptyListException</c> class with defined default message by constructor.</para>
        /// <para><b>TR: </b>Yapıcı tarafından tanımlanan varsayılan mesajla <c> ABKExceptionLibrary.EmptyListException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public EmptyListException(IStringLocalizer localizer) : base(localizer["EmptyListException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.EmptyListException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.EmptyListException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public EmptyListException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.EmptyListException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata iletisiyle <c> ABKExceptionLibrary.EmptyListException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public EmptyListException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.EmptyList;
        }
    }
}
