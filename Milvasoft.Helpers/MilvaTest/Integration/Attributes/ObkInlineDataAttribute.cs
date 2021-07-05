using Milvasoft.Helpers.MilvaTest.Integration.Enums;
using Milvasoft.Helpers.MilvaTest.Integration.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using Xunit.Sdk;

namespace Milvasoft.Helpers.MilvaTest.Integration.Attributes
{
    /// <summary>
    /// Test methods attribute for integration test.
    /// </summary>
    public class ObkInlineDataAttribute : DataAttribute
    {
        private readonly string _url;
        private readonly UrlTypeEnum _obkInlineDataEnum;
        private readonly HttpMethod _httpMethod;
        private readonly object _specificObj;

        /// <summary>
        /// Constructor of <see cref="ObkInlineDataAttribute"/>
        /// </summary>
        public ObkInlineDataAttribute()
        {
            _url = "";
            _httpMethod = HttpMethod.Get;
            _obkInlineDataEnum = UrlTypeEnum.InController;
            _specificObj = null;
        }

        /// <summary>
        /// Constructor of <see cref="ObkInlineDataAttribute"/>
        /// </summary>
        public ObkInlineDataAttribute(string url, string httpMethod, UrlTypeEnum obkInlineDataEnum = UrlTypeEnum.InController, object specificObj = null)
        {
            _url = url;
            _httpMethod = new HttpMethod(httpMethod);
            _obkInlineDataEnum = obkInlineDataEnum;
            _specificObj = specificObj;
        }

        /// <summary>
        /// Returns the data to be used to test the theory
        /// </summary>
        /// <param name="testMethod"></param>
        /// <returns></returns>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var controllerName = testMethod.ReflectedType.Name.Split("Controller")[0];
            string url = _url;

            if (_obkInlineDataEnum == UrlTypeEnum.InController)
                url = $"{controllerName}/{_url}";

            var language = StaticMethods.GetRandomLanguageForTest();

            yield return new object[] { TestInject.GetTestInject(url, _httpMethod, new CultureInfo(language).GetSpecificStringLocalizer(), language, _specificObj) };
        }
    }
}
