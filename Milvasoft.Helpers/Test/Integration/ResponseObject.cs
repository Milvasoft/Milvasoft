namespace Milvasoft.Helpers.Test.Integration;

/// <summary>
/// Response object model for test.
/// </summary>
public class ResponseObject
{
    /// <summary>
    /// Constructor of <see cref="ResponseObject"/>.
    /// </summary>
    private ResponseObject() { }

    /// <summary>
    /// Status code of expected.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Successful of expected.
    /// </summary>
    public bool? Successful { get; init; }

    /// <summary>
    /// Message key of expected.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Returns <see cref="TestExpectected"/> instance.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="isSuccesful"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ResponseObject GetTestExpectectedInstance(int? statusCode, bool? isSuccesful, string message)
    {
        return new ResponseObject
        {
            Message = message,
            StatusCode = statusCode,
            Successful = isSuccesful
        };
    }
}
