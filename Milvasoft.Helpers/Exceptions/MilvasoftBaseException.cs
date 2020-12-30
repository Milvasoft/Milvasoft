using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    public abstract class MilvasoftBaseException : Exception
    {
        /// <summary>
        /// <para><b>EN: </b> Initializes a new instance of the <c>Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b><c> Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini belirtilen bir hata mesajıyla başlatır.</para>
        /// </summary>
        /// <param name="localizer"></param>
        public MilvasoftBaseException(IStringLocalizer localizer) : base(localizer["OpsiyonBaseException"])
        {

        }

        /// <summary>
        /// <para><b>EN: </b> Initializes a new instance of the <c>Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b><c> Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini belirtilen bir hata mesajıyla başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public MilvasoftBaseException(string customMessage) : base(customMessage)
        {

        }

        /// <summary>
        /// <para><b>EN: </b> Initializes a new instance of the <c>Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException</c> class  with a specified error message and a reference to the inner exception that is the cause of this exception.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> Milvasoft.Helpers.Exceptions.AddingNewEntityWithExistsIdException </c> sınıfının yeni bir örneğini ve bu istisnanın nedeni olan iç özel duruma bir başvuruyu başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public MilvasoftBaseException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {

        }

        /// <summary>
        /// <para><b>EN: </b>Defines error code.</para>
        /// <para><b>TR: </b>Hata kodunu tanımlar.</para>
        /// </summary>
        public int ErrorCode { get; set; }
    }
}
