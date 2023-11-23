using Milvasoft.Core.Utils.Constants;
using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Models.Response;

/// <summary>
/// Base response model for API responses.
/// </summary>
public abstract class BaseResponse
{
    /// <summary>
    /// <para> Default = true. </para>
    /// <para> Information of whether the request was successful. </para>
    /// <para> If request is success: Success = true. </para>
    /// <para> If request is fail: Success = false. </para>
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// <para> Default: Process Successful! </para>
    /// <para> Message of response. </para>
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Process Successful!";

    /// <summary>
    /// <para> Default = <see cref="MilvaStatusCodes.Status200OK"/> </para>
    /// <para> Http status code of response. </para>
    /// </summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; } = MilvaStatusCodes.Status200OK;

}
