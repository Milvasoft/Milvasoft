using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Test.Integration
{
    /// <summary>
    /// Fake client for integration test.
    /// </summary>
    public abstract class MilvaTestClient<TStartup> where TStartup : MilvaTestStartup
    {
        private readonly HttpClient _httpClient;
        private readonly TestServer _testServer;

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
        /// Function that returns token for security tests.
        /// </summary>
        public static Func<object, Task<string>> GetTokenAsync { get; set; }

        /// <summary>
        /// Constructor of <see cref="MilvaTestClient{TStartup}"/>.
        /// </summary>
        /// <param name="acceptedLanguageIsoCodes"></param>
        /// <param name="acceptedRoles"></param>
        /// <param name="localizerResourceSource"></param>
        /// <param name="testApiBaseUrl"></param>
        /// <param name="loginUrl"></param>
        /// <param name="testEnvironment"></param>
        /// <param name="loginDtoAndUserName"></param>
        /// <param name="userManager"></param>
        /// <param name="getTokenAsync"></param>
        public MilvaTestClient(List<string> acceptedLanguageIsoCodes,
                               List<string> acceptedRoles,
                               Type localizerResourceSource,
                               string testApiBaseUrl,
                               string loginUrl,
                               string testEnvironment,
                               (object, string) loginDtoAndUserName,
                               Type userManager,
                               Func<object, Task<string>> getTokenAsync)
        {
            if (_testServer == null)
                _testServer = new TestServer(new WebHostBuilder().UseStartup<TStartup>().UseEnvironment(testEnvironment));

            _httpClient = _testServer.CreateClient();

            MilvaTestClient<MilvaTestStartup>.GetTokenAsync = getTokenAsync;
            MilvaTestClient<MilvaTestStartup>.AcceptedLanguageIsoCodes = acceptedLanguageIsoCodes;
            MilvaTestClient<MilvaTestStartup>.AcceptedRoles = acceptedRoles;
            MilvaTestClient<MilvaTestStartup>.LocalizerResourceSource = localizerResourceSource;
            MilvaTestClient<MilvaTestStartup>.TestApiBaseUrl = testApiBaseUrl;
            MilvaTestClient<MilvaTestStartup>.LoginUrl = loginUrl;
            MilvaTestClient<MilvaTestStartup>.HttpClient = _httpClient;
            MilvaTestClient<MilvaTestStartup>.LoginDtoAndUserName = loginDtoAndUserName;
            MilvaTestClient<MilvaTestStartup>.UserManager = _testServer.Services.GetRequiredService(userManager);
        }

        /// <summary>
        /// Returns <see cref="HttpClient"/>.
        /// </summary>
        /// <returns></returns>
        public HttpClient GetHttpClient() => _httpClient;

        /// <summary>
        /// Returns <see cref="IServiceProvider"/>.
        /// </summary>
        /// <returns></returns>
        public IServiceProvider GetServiceProvider() => _testServer.Services;

        /// <summary>
        /// Returns service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetService<TService>() => _testServer.Services.GetRequiredService<TService>();

        /// <summary>
        /// Returns service.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public object GetService(Type service) => _testServer.Services.GetRequiredService(service);

        /// <summary>
        /// Updates language at runtime
        /// </summary>
        /// <param name="language"></param>
        public void SetLanguage(string language)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(language);
            CultureInfo.CurrentCulture = new CultureInfo(language);
        }

        ///<summary>
        /// Reset database for every test methods.
        /// </summary>
        /// <returns></returns>
        public abstract Task ResetDatabaseAsync();
    }
}
