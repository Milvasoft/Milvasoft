using Newtonsoft.Json;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// Response model for requests that return a object.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ObjectResponse<TEntity> : BaseResponse
    {
        /// <summary>
        /// Result object.
        /// </summary>
        [JsonProperty("result")]
        public virtual TEntity Result { get; set; }
    }
}
