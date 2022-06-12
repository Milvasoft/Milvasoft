﻿using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milvasoft.Core.Exceptions;
using Milvasoft.Testing.Helpers;
using Milvasoft.Testing.IntegrationTest.Attributes;
using Milvasoft.Testing.IntegrationTest.TestStartup.Abstract;
using System.Globalization;
using System.Reflection;

namespace Milvasoft.Testing.IntegrationTest.Utils;

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
        var getInstanceMethodName = (string)clientAttribute.ConstructorArguments[1].Value;

        if (string.IsNullOrWhiteSpace(getInstanceMethodName))
            throw new MilvaTestException("Please enter the method name that generates the client instance.");

        var info = clientType.GetMethod(getInstanceMethodName);

        info.IsNull("The method that generated the client instance could not be found.");

        _ = info.Invoke(null, null);
    }
}