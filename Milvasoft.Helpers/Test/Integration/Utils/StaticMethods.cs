using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milvasoft.Helpers.Test.Helpers;
using Milvasoft.Helpers.Test.Integration.Attributes;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Milvasoft.Helpers.Test.Integration.Utils
{
    /// <summary>
    /// Helper static methods for integration tests.
    /// </summary>
    internal static class StaticMethods
    {
        /// <summary>
        /// Returns random language for integration test response message.
        /// </summary>
        /// <returns></returns>
        internal static string GetRandomLanguageForTest()
        {
            Random random = new();

            var acceptedLanguages = MilvaTestClient<MilvaTestStartup>.AcceptedLanguageIsoCodes;

            acceptedLanguages.IsNull("Please enter the iso codes for the accepted languages.");

            acceptedLanguages.Trim();

            var randomIndex = random.Next(0, acceptedLanguages.Count);
            return acceptedLanguages[randomIndex];
        }

        /// <summary>
        /// Returns specific string localizer.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        internal static IStringLocalizer GetSpecificStringLocalizer(this CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;

            CultureInfo.CurrentUICulture = cultureInfo;

            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });

            var factory = new ResourceManagerStringLocalizerFactory(options, new LoggerFactory());

            var localizerResource = MilvaTestClient<MilvaTestStartup>.LocalizerResourceSource;

            localizerResource.IsNull("Please enter the localizer resource type.");

            return factory.Create(localizerResource);
        }

        /// <summary>
        /// Creates client for integration test.
        /// </summary>
        /// <param name="methodInfo"></param>
        internal static void CreateClientInstance(this MethodInfo methodInfo)
        {
            var clientAttribute = methodInfo.ReflectedType.CustomAttributes.ToList().FirstOrDefault(p => p.AttributeType == typeof(CreateClientAttribute));

            clientAttribute.IsNull("Please use 'CreateClientAttribute' in your test class.");

            var clientType = (Type)clientAttribute.ConstructorArguments[0].Value;
            var getInstanceMethodName = "GetFakeClientInstance";

            var info = clientType.GetMethod(getInstanceMethodName);

            _ = info.Invoke(null, null);
        }
    }
}
