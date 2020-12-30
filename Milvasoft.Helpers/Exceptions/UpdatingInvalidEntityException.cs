using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to update an entity that is not in the Repository.</para>
    /// <para><b>TR: </b>Havuzda olmayan bir varlığı güncelleme girişiminde bulunulduğunda ortaya çıkan istisna.</para>
    /// </summary>
    public class UpdatingInvalidEntityException : MilvasoftBaseException
    {

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.UpdatingInvalidEntityException</c> class with an entity id not found in the Repository. The spesific message of base class uses this invalid id when does not send a specific message.</para>
        /// <para><b>TR: </b>Depoda bulunmayan bir varlık kimliğiyle <c> ABKExceptionLibrary.UpdatingInvalidEntityException </c> sınıfının yeni bir örneğini başlatır. Temel sınıfın özel mesajı, belirli bir mesaj göndermediğinde bu geçersiz kimliği kullanır.</para>
        /// </summary>
        /// <param name="localizer"></param>
        public UpdatingInvalidEntityException(IStringLocalizer localizer) : base(localizer["UpdatingInvalidEntityException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.UpdatingInvalidEntity;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.UpdatingInvalidEntityException</c> class with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.UpdatingInvalidEntityException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public UpdatingInvalidEntityException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.UpdatingInvalidEntity;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.UpdatingInvalidEntityException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.UpdatingInvalidEntityException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public UpdatingInvalidEntityException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.UpdatingInvalidEntity;
        }

    }
}
