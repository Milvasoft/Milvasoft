using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Test.Helpers;
using Milvasoft.Helpers.Test.Integration.Enums;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using Milvasoft.Helpers.Test.Integration.Utils;
using Milvasoft.Helpers.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Xunit.Sdk;

namespace Milvasoft.Helpers.Test.Integration.Attributes;

/// <summary>
/// Test methods attribute for integration security testing.
/// </summary>
public class MilvaSecurityTestInjectAttribute : DataAttribute
{
    private readonly string _url;
    private readonly AuthorizeTypeEnum _authorizeTypeEnum;
    private readonly UrlTypeEnum _obkInlineDataEnum;
    private readonly HttpMethod _httpMethod;
    private readonly List<string> _acceptedRoles;

    /// <summary>
    /// Constructor of <see cref="MilvaSecurityTestInjectAttribute"/>.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpMethod"></param>
    /// <param name="acceptedRoles"></param>
    /// <param name="authorizeTypeEnum"></param>
    /// <param name="obkInlineDataEnum"></param>
    public MilvaSecurityTestInjectAttribute(string url, string httpMethod, string acceptedRoles, AuthorizeTypeEnum authorizeTypeEnum = AuthorizeTypeEnum.Or, UrlTypeEnum obkInlineDataEnum = UrlTypeEnum.InController)
    {
        _url = url;
        _httpMethod = new HttpMethod(httpMethod);
        _obkInlineDataEnum = obkInlineDataEnum;
        _authorizeTypeEnum = authorizeTypeEnum;
        _acceptedRoles = string.IsNullOrWhiteSpace(acceptedRoles) ? new List<string>() : acceptedRoles.Split(',').ToList();
    }

    /// <summary>
    /// Returns the data to be used to test the theory
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public override IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        methodInfo.CreateClientInstance();

        string url = _url;

        if (_obkInlineDataEnum == UrlTypeEnum.InController)
        {
            var controllerName = methodInfo.ReflectedType.Name.Split("Controller")[0];
            url = $"{controllerName}/{_url}";
        }

        var allRoles = MilvaTestClient<MilvaTestStartup>.AcceptedRoles;

        allRoles.IsNull("Please enter application Roles.");

        _acceptedRoles.Trim();
        allRoles.Trim();

        TestExpectected testExpectected;

        var testLanguage = StaticMethods.GetRandomLanguageForTest();

        if (_authorizeTypeEnum == AuthorizeTypeEnum.Or)
        {
            foreach (var role in allRoles)
            {
                var isAccepted = _acceptedRoles.Any(p => p == role);

                var token = RequestHelper.GetTokenByRoles(role).GetAwaiter().GetResult();

                if (isAccepted)
                {
                    testExpectected = TestExpectected.GetTestExpectectedInstance(null, null, null);

                    yield return new object[] { SafetyTestInject.GetSafetyTestInject(url, _httpMethod, new CultureInfo(testLanguage).GetSpecificStringLocalizer(), testLanguage, testExpectected, isAccepted, token) };
                }
                else
                {
                    testExpectected = TestExpectected.GetTestExpectectedInstance(statusCode: MilvaStatusCodes.Status403Forbidden, isSuccesful: false, messageKey: "Forbidden");

                    yield return new object[] { SafetyTestInject.GetSafetyTestInject(url, _httpMethod, new CultureInfo(testLanguage).GetSpecificStringLocalizer(), testLanguage, testExpectected, isAccepted, token) };
                }
            }

            testExpectected = TestExpectected.GetTestExpectectedInstance(statusCode: MilvaStatusCodes.Status401Unauthorized, isSuccesful: false, messageKey: "Unauthorized");

            yield return new object[] { SafetyTestInject.GetSafetyTestInject(url, _httpMethod, new CultureInfo(testLanguage).GetSpecificStringLocalizer(), testLanguage, testExpectected, false, null) };
        }
        else if (_authorizeTypeEnum == AuthorizeTypeEnum.And)
        {
            throw new MilvaDeveloperException("This option is not ready yet.");
        }
        else if (_authorizeTypeEnum == AuthorizeTypeEnum.None)
        {
            testExpectected = TestExpectected.GetTestExpectectedInstance(null, null, null);

            yield return new object[] { SafetyTestInject.GetSafetyTestInject(url, _httpMethod, new CultureInfo(testLanguage).GetSpecificStringLocalizer(), testLanguage, testExpectected, true, null) };
        }
    }
}
