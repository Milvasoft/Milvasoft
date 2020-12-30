using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to search for an entity not found in the Repository.</para>
    /// <para><b>TR: </b>Havuzda bulunmayan bir varlığı arama girişiminde bulunulduğunda ortaya çıkan istisna.</para>
    /// </summary>
    public class CannotFindEntityException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEntityException</c> class with defined default message by constructor.</para>
        /// <para><b>TR: </b>Yapıcı tarafından tanımlanan varsayılan mesajla <c> ABKException Library.Canot Find EntityException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public CannotFindEntityException(IStringLocalizer localizer) : base(localizer["CannotFindEntityException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEntity;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEntityException</c> class with an entity id not found in the Repository. The spesific message of base class uses this invalid id when does not send a specific message.</para>
        /// <para><b>TR: </b>Havuzda bulunmayan bir varlık kimliğiyle <c> ABKExceptionLibrary.CannotFindEntityException </c> sınıfının yeni bir örneğini başlatır. Temel sınıfın özel mesajı, belirli bir mesaj göndermediğinde bu geçersiz kimliği kullanır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public CannotFindEntityException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEntity;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.CannotFindEntityException</c> class with an entity id not found in the Repository. The spesific message of base class uses this invalid id when does not send a specific message.</para>
        /// <para><b>TR: </b>Havuzda bulunmayan bir varlık kimliğiyle <c> ABKExceptionLibrary.CannotFindEntityException </c> sınıfının yeni bir örneğini başlatır. Temel sınıfın özel mesajı, belirli bir mesaj göndermediğinde bu geçersiz kimliği kullanır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public CannotFindEntityException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.CannotFindEntity;
        }
    }
}
