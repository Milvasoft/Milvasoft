using Microsoft.Extensions.Localization;
using System;

namespace Milvasoft.Helpers.Exceptions
{
    public class DeletingEntityWithRelationsException : MilvasoftBaseException
    {
        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.DeletingEntityWithRelationsException</c> class.</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.DeletingEntityWithRelationsException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        public DeletingEntityWithRelationsException(IStringLocalizer localizer) : base(localizer["DeletingEntityWithRelationsException"])
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.DeletingEntityWithRelationsException</c> class  with a specified error message.</para>
        /// <para><b>TR: </b>Belirtilen bir hata mesajıyla <c> ABKExceptionLibrary.DeletingEntityWithRelationsException </c> sınıfının yeni bir örneğini başlatır.</para>
        /// </summary>
        /// <param name="customMessage"></param>
        public DeletingEntityWithRelationsException(string customMessage) : base(customMessage)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }

        /// <summary>
        /// <para><b>EN: </b>Initializes a new instance of the <c>ABKExceptionLibrary.DeletingEntityWithRelationsException</c> class</para>
        /// <para><b>TR: </b><c> ABKExceptionLibrary.DeletingEntityWithRelationsException </c> sınıfının yeni bir örneğini başlatır</para>
        /// </summary>
        /// <param name="customMessage"></param>
        /// <param name="innerException"></param>
        public DeletingEntityWithRelationsException(string customMessage, Exception innerException) : base(customMessage, innerException)
        {
            ErrorCode = (int)MilvasoftExceptionCode.DeletingEntityWithRelations;
        }
    }
}
