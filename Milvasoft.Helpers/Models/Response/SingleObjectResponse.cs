using Newtonsoft.Json;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para><b>EN: </b> Response model for requests that return a single object.</para>
    /// <para><b>TR: </b>Tek bir nesne döndüren istekler için yanıt modeli.</para>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SingleObjectResponse<TEntity> : BaseResponse
    {
        /// <summary>
        /// <para><b>EN: </b>Result object.</para>
        /// <para><b>TR: </b>Sonuç nesnesi.</para>
        /// </summary>
        [JsonProperty("result")]
        public virtual TEntity Result { get; set; }
    }
}
