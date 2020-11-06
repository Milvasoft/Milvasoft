using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para><b>EN: </b> Validation error model for property.</para>
    /// <para><b>TR: </b>Mülk için doğrulama hata modeli.</para>
    /// </summary>
    public class ValidationError
    {
        /// <value>
        /// <para><b>EN: </b>Property where validation error occurred. </para>
        /// <para><b>TR: </b>Doğrulama hatasının oluştuğu mülk.</para>
        /// </value>
        [JsonProperty("validationFieldName")]
        public string ValidationFieldName { get; set; }

        /// <value>
        /// <para><b>EN: </b>Property's validation error messages</para>
        /// <para><b>TR: </b>Mülkün doğrulama hata mesajları</para>
        /// </value>
        [JsonProperty("errorMessageList")]
        public List<string> ErrorMessageList { get; set; }
    }
}
