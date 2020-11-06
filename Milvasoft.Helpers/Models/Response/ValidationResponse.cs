using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para> Response model for requests that return a validation error.</para>
    /// </summary>
    public class ValidationResponse : BaseResponse
    {
        /// <summary>
        /// <para> Result of request </para>
        /// </summary>
        [JsonProperty("result")]        
        public object Result { get; set; }

        /// <summary>
        /// <para> Error codes of request, if exists. </para>
        /// </summary>
        [JsonProperty("errorCodes")]
        public List<int> ErrorCodes { get; set; }
    }
}

