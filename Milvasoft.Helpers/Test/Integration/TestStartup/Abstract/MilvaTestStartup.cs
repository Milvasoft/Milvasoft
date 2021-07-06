using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Milvasoft.Helpers.Test.Integration.TestStartup.Abstract
{
    /// <summary>
    /// Fake startup for integration test.
    /// </summary>
    public abstract class MilvaTestStartup
    {
        /// <summary>
        /// Supported languages for the project.
        /// </summary>
        public static List<string> AcceptedLanguageIsoCodes { get; private set; }

        /// <summary>
        /// Supported roles for the project.
        /// </summary>
        public static List<string> AcceptedRoles { get; private set; }

        /// <summary>
        /// Localizer resource.
        /// </summary>
        public static Type LocalizerResourceSource { get; private set; }

        /// <summary>
        /// Integration test base url.
        /// </summary>
        public static string TestApiBaseUrl { get; set; }

        /// <summary>
        /// Url defined for user input
        /// </summary>
        public static string LoginUrl { get; set; }

        /// <summary>
        /// Test environemnt.
        /// </summary>
        public static string TestEnvironment { get; set; }

        /// <summary>
        /// Http client instance.
        /// </summary>
        public static HttpClient HttpClient { get; set; }

        /// <summary>
        /// User input data required for security tests.
        /// </summary>
        public static (object, string) LoginDtoAndUserName { get; set; }

        /// <summary>
        /// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/> instance
        /// </summary>
        public static object UserManager { get; set; }

        /// <summary>
        /// The JSON name of the token property contained in response, which will be returned by the Api.
        /// </summary>
        public static string TokenPropName { get; set; }

        /// <summary>
        /// Constructor of <see cref="MilvaTestStartup"/>.
        /// </summary>
        /// <param name="acceptedLanguageIsoCodes"></param>
        /// <param name="acceptedRoles"></param>
        /// <param name="localizerResourceSource"></param>
        /// <param name="testApiBaseUrl"></param>
        /// <param name="loginUrl"></param>
        /// <param name="testEnvironment"></param>
        /// <param name="httpClient"></param>
        /// <param name="loginDtoAndUserName"></param>
        /// <param name="userManager"></param>
        /// <param name="tokenPropName"></param>
        public MilvaTestStartup(List<string> acceptedLanguageIsoCodes,
                                List<string> acceptedRoles,
                                Type localizerResourceSource,
                                string testApiBaseUrl,
                                string loginUrl,
                                string testEnvironment,
                                HttpClient httpClient,
                                (object, string) loginDtoAndUserName,
                                object userManager,
                                string tokenPropName)
        {
            AcceptedLanguageIsoCodes = acceptedLanguageIsoCodes;
            AcceptedRoles = acceptedRoles;
            LocalizerResourceSource = localizerResourceSource;
            TestApiBaseUrl = testApiBaseUrl;
            LoginUrl = loginUrl;
            TestEnvironment = testEnvironment;
            HttpClient = httpClient;
            LoginDtoAndUserName = loginDtoAndUserName;
            UserManager = userManager;
            TokenPropName = tokenPropName;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public abstract void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public abstract void Configure(IApplicationBuilder app);
    }
}
