using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// <para><b>EN: </b>The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.</para>
    /// <para><b>TR: </b>Depo kimliğine sahip bir varlık ekleme girişiminde bulunulduğunda ortaya çıkan istisna, aynı kimliğe sahip bir varlık içerir.</para>
    /// </summary>
    public class AddingNewEntityWithExistsIdException : MilvasoftBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <c>ABKExceptionLibrary.AddingNewEntityWithExistsIdException</c> class.
        /// </summary>
        public AddingNewEntityWithExistsIdException(IStringLocalizer localizer) : base(localizer["AddingNewEntityWithExistsIdException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.AddingNewEntityWithExsistId;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.AddingNewEntityWithExistsIdException</c> class with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajı ile <c> ABKExceptionLibrary.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customMessage"></param>
        public AddingNewEntityWithExistsIdException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.AddingNewEntityWithExsistId;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.AddingNewEntityWithExistsIdException</c> class with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public AddingNewEntityWithExistsIdException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.AddingNewEntityWithExsistId;
        }
    }
}
