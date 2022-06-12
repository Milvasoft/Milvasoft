using Microsoft.Extensions.Localization;

namespace Milvasoft.Testing.IntegrationTest;

/// <summary>
/// Helper record for safety tests.
/// </summary>
public record SafetyTestInject
{
    /// <summary>
    /// Consturctor of <see cref="SafetyTestInject"/>.
    /// </summary>
    private SafetyTestInject() { }

    /// <summary>
    /// The url to which the request will be sent.
    /// </summary>
    public string Url { get; init; }

    /// <summary>
    /// Method type of request.
    /// </summary>
    public HttpMethod HttpMethod { get; init; }

    /// <summary>
    /// The language in which the request will be sent. 
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// Localizer instance for response equals.
    /// </summary>
    public IStringLocalizer StringLocalizer { get; init; }

    /// <summary>
    /// Test excepted record.
    /// </summary>
    public TestExpectected TestExpectected { get; init; }

    /// <summary>
    /// Whether the role value being tested is accepted
    /// </summary>
    public bool IsAcceptedRole { get; init; }

    /// <summary>
    /// The token to be added when sending the request.
    /// </summary>
    public string Token { get; init; }

    /// <summary>
    /// Returns <see cref="SafetyTestInject"/> instance.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpMethod"></param>
    /// <param name="stringLocalizer"></param>
    /// <param name="language"></param>
    /// <param name="testExpectected"></param>
    /// <param name="isAccepted"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static SafetyTestInject GetSafetyTestInject(string url, HttpMethod httpMethod, IStringLocalizer stringLocalizer, string language, TestExpectected testExpectected, bool isAccepted, string token)
    {
        return new SafetyTestInject
        {
            HttpMethod = httpMethod,
            IsAcceptedRole = isAccepted,
            Language = language,
            StringLocalizer = stringLocalizer,
            TestExpectected = testExpectected,
            Token = token,
            Url = url
        };
    }
}
