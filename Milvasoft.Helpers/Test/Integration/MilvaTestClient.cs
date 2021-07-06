using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Test.Integration.TestStartup.Abstract;
using System;
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
        /// Constructor of <see cref="MilvaTestClient{TStartup}"/>.
        /// </summary>
        public MilvaTestClient()
        {
            if (_testServer == null)
                _testServer = new TestServer(new WebHostBuilder().UseStartup<TStartup>().UseEnvironment(MilvaTestStartup.TestEnvironment));

            _httpClient = _testServer.CreateClient();
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
