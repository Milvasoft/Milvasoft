using System.Text.Json.Serialization;

namespace Milvasoft.Components.Rest.MilvaResponse;

/// <summary>
/// Represents a response interface containing properties and methods to handle response data and messages.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the response is successful.
    /// </summary>
    bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets a status code indicating whether the response is successful.
    /// </summary>
    int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the list of response messages.
    /// </summary>
    List<ResponseMessage> Messages { get; set; }

    /// <summary>
    /// Throws an exception if the response is not successful.
    /// </summary>
    void ThrowExceptionIfFail();

    /// <summary>
    /// Converts the response messages to a string with the specified delimiter.
    /// </summary>
    /// <param name="delimiter">The delimiter to use for separating messages (default is ';').</param>
    /// <returns>A string representation of the response messages.</returns>
    string MessagesToString(string delimiter = ";");
}

/// <summary>
/// Represents a response interface containing properties and methods to handle response data and messages.
/// </summary>
public interface IResponse<T> : IResponse, IHasMetadata
{
    /// <summary>
    /// Gets or sets response data.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Specifies the response data is cached or not.
    /// </summary>
    [JsonIgnore]
    bool IsCachedData { get; set; }
}

/// <summary>
/// Represents an interface for a response that contains metadata about the response data.
/// </summary>
public interface IHasMetadata
{
    /// <summary>
    /// Gets or sets the list of column types in the response data.
    /// </summary>
    List<ResponseDataMetadata> Metadatas { get; set; }

    /// <summary>
    /// Retrieves the data and its corresponding type from the response.
    /// </summary>
    /// <returns>A tuple containing the data object and its type.</returns>
    (object Data, Type DataType) GetResponseDataTypePair();
}