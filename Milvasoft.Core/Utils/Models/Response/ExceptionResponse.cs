using Newtonsoft.Json;

namespace Milvasoft.Core.Utils.Models.Response;

/// <summary>
/// Model of Exception.
/// </summary>
public class ExceptionResponse : ObjectResponse<object>
{
    /// <summary>
    /// Result of request.
    /// </summary>
    [JsonProperty("result")]
    public override object Result { get; set; }

    /// <summary>
    /// Error codes of request, if exists.
    /// </summary>
    [JsonProperty("errorCodes")]
    public List<int> ErrorCodes { get; set; }
}
