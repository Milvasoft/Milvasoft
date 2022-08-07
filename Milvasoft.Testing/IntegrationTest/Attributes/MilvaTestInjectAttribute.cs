using Milvasoft.Testing.IntegrationTest.Enums;
using Milvasoft.Testing.IntegrationTest.Utils;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Milvasoft.Testing.IntegrationTest.Attributes;

/// <summary>
/// Test methods attribute for integration test.
/// </summary>
public class MilvaTestInjectAttribute : DataAttribute
{
    private readonly string _url;
    private readonly UrlTypeEnum _urlTypeEnum;
    private readonly HttpMethod _httpMethod;
    private readonly object _specificObj;

    /// <summary>
    /// Constructor of <see cref="MilvaTestInjectAttribute"/>
    /// </summary>
    public MilvaTestInjectAttribute()
    {
        _url = "";
        _httpMethod = HttpMethod.Get;
        _urlTypeEnum = UrlTypeEnum.InController;
        _specificObj = null;
    }

    /// <summary>
    /// Constructor of <see cref="MilvaTestInjectAttribute"/>
    /// </summary>
    public MilvaTestInjectAttribute(string url, string httpMethod, UrlTypeEnum obkInlineDataEnum = UrlTypeEnum.InController, object specificObj = null)
    {
        _url = url;
        _httpMethod = new HttpMethod(httpMethod);
        _urlTypeEnum = obkInlineDataEnum;
        _specificObj = specificObj;
    }

    /// <summary>
    /// Returns the data to be used to test the theory
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public override IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        methodInfo.CreateClientInstance();

        var controllerName = methodInfo.ReflectedType.Name.Split("Controller")[0];
        string url = _url;

        if (_urlTypeEnum == UrlTypeEnum.InController)
            url = $"{controllerName}/{_url}";

        var language = StaticMethods.GetRandomLanguageForTest();

        yield return new object[] { TestInject.GetTestInject(url, _httpMethod, new CultureInfo(language).GetSpecificStringLocalizer(), language, _specificObj) };
    }
}
