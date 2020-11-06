using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para><b>EN: </b>Model of Exception.</para>
    /// <para><b>TR: </b>İstisna Modeli.</para>
    /// </summary>
    public class ExceptionResponse : SingleObjectResponse<object>
    {
        /// <summary>
        /// <para><b>EN: </b>Result of request.</para>
        /// <para><b>TR: </b>Talebin sonucu.</para>
        /// </summary>
        [JsonProperty("result")]
        public override object Result { get; set; }

        /// <summary>
        /// <para><b>EN: </b>Error codes of request, if exists.</para>
        /// <para><b>TR: </b>Varsa, isteğin hata kodları.</para>
        /// </summary>
        [JsonProperty("errorCodes")]
        public List<int> ErrorCodes { get; set; }
    }
}
