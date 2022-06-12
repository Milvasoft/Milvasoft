namespace Milvasoft.Testing.IntegrationTest;

/// <summary>
/// Test result class for safety testing.
/// </summary>
public record TestExpectected
{
    /// <summary>
    /// Constructor of <see cref="TestExpectected"/>.
    /// </summary>
    private TestExpectected() { }

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
    public string MessageKey { get; init; }

    /// <summary>
    /// Returns <see cref="TestExpectected"/> instance.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="isSuccesful"></param>
    /// <param name="messageKey"></param>
    /// <returns></returns>
    public static TestExpectected GetTestExpectectedInstance(int? statusCode, bool? isSuccesful, string messageKey)
    {
        return new TestExpectected
        {
            MessageKey = messageKey,
            StatusCode = statusCode,
            Successful = isSuccesful
        };
    }
}
