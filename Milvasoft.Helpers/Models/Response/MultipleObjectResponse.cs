using Newtonsoft.Json;
using System.Collections.Generic;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para><b>EN: </b>Response model for requests that return a multiple object(List).</para>
    /// <para><b>TR: </b>Birden çok nesne (Liste) döndüren istekler için yanıt modeli.</para>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MultipleObjectResponse<TEntity> : BaseResponse
    {
        /// <summary>
        /// <para><b>EN: </b>Result list. </para>
        /// <para><b>TR: </b>Sonuç listesi.</para>
        /// </summary>
        [JsonProperty("result")]
        public IEnumerable<TEntity> Result { get; set; }
    }
}
