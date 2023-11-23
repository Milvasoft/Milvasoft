using System.Text.Json.Serialization;

namespace Milvasoft.Core.Utils.Models.Response;

/// <summary>
/// Response model for requests that return a object.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class ObjectResponse<TEntity> : BaseResponse
{
    /// <summary>
    /// Result object.
    /// </summary>
    [JsonPropertyName("result")]
    public virtual TEntity Result { get; set; }
}
