using Newtonsoft.Json;

namespace Milvasoft.Helpers.Models.Response
{
    /// <summary>
    /// <para><b>EN: </b>Base response model for API responses.</para>
    /// <para><b>TR: </b>API yanıtları için temel yanıt modeli.</para>
    /// </summary>
    public abstract class BaseResponse
    {
        /// <summary>
        /// <para> Default = true. </para>
        /// <para> Information of whether the request was successful. </para>
        /// <para> If request is success: Success = true. </para>
        /// <para> If request is fail: Success = false. </para>
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        /// <summary>
        /// <para> Default: İşlem başarılı!! </para>
        /// <para> Message of response. </para>
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = "Process Successful!";

        /// <summary>
        /// <para> Default = <see cref="MilvasoftStatusCodes.Status200OK"/> </para>
        /// <para> Http status code of response. </para>
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; } = MilvasoftStatusCodes.Status200OK;

    }
}
