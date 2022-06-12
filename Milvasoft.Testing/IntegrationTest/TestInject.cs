using Microsoft.Extensions.Localization;
using System.Net.Http;

namespace Milvasoft.Testing.IntegrationTest;

/// <summary>
/// A record that contains parameters that test methods depend on.
/// </summary>
public record TestInject
{
    /// <summary>
    /// Cosntructor of <see cref="TestInject"/>.
    /// </summary>
    private TestInject() { }

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
    /// Special object for test methods.
    /// </summary>
    public object SpecificObject { get; init; }

    /// <summary>
    /// Returns <see cref="TestInject"/> instance.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpMethod"></param>
    /// <param name="stringLocalizer"></param>
    /// <param name="language"></param>
    /// <param name="specificObject"></param>
    /// <returns></returns>
    public static TestInject GetTestInject(string url, HttpMethod httpMethod, IStringLocalizer stringLocalizer, string language, object specificObject = null)
    {
        return new TestInject
        {
            HttpMethod = httpMethod,
            Language = language,
            StringLocalizer = stringLocalizer,
            Url = url,
            SpecificObject = specificObject
        };
    }
}
