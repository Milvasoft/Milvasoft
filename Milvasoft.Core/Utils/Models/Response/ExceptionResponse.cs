using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Models.Response;

/// <summary>
/// Model of Exception.
/// </summary>
public class ExceptionResponse : ObjectResponse<object>
{
    /// <summary>
    /// Result of request.
    /// </summary>
    [JsonPropertyName("result")]
    public override object Result { get; set; }

    /// <summary>
    /// Error codes of request, if exists.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    public List<int> ErrorCodes { get; set; }
}
