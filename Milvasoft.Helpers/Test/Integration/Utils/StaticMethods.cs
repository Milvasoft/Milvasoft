﻿using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using System;
using System.Globalization;

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

            return factory.Create(MilvaTestClient<MilvaTestStartup>.LocalizerResourceSource);
        }
    }
}
