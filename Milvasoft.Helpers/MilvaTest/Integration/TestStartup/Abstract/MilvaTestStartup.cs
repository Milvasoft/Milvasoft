using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Milvasoft.Helpers.MilvaTest.Integration.TestStartup.Abstract
{
    /// <summary>
    /// Fake startup for integration test.
    /// </summary>
    public abstract class MilvaTestStartup
    {
        /// <summary>
        /// Supported languages for the project.
        /// </summary>
        public static List<string> _acceptedLanguageIsoCodes { get; private set; }

        /// <summary>
        /// Supported roles for the project.
        /// </summary>
        public static List<string> _acceptedRoles { get; private set; }

        /// <summary>
        /// Localizer resource.
        /// </summary>
        public static Type _resourceSource { get; private set; }

        /// <summary>
        /// Integration test base url.
        /// </summary>
        public static string _testApiBaseUrl { get; set; }

        /// <summary>
        /// Url defined for user input
        /// </summary>
        public static string _loginUrl { get; set; }

        /// <summary>
        /// Test environemnt.
        /// </summary>
        public static string _testEnvironment { get; set; }

        /// <summary>
        /// Constructor of <see cref="MilvaTestStartup"/>.
        /// </summary>
        /// <param name="acceptedLanguageIsoCodes"></param>
        /// <param name="acceptedRoles"></param>
        /// <param name="resourceSource"></param>
        /// <param name="testApiBaseUrl"></param>
        /// <param name="loginUrl"></param>
        /// <param name="testEnvironment"></param>
        public MilvaTestStartup(List<string> acceptedLanguageIsoCodes,
                                List<string> acceptedRoles,
                                Type resourceSource,
                                string testApiBaseUrl,
                                string loginUrl,
                                string testEnvironment)
        {
            _acceptedLanguageIsoCodes = acceptedLanguageIsoCodes;
            _acceptedRoles = acceptedRoles;
            _resourceSource = resourceSource;
            _testApiBaseUrl = testApiBaseUrl;
            _loginUrl = loginUrl;
            _testEnvironment = testEnvironment;
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
        /// <param name="env"></param>
        public abstract void Configure(IApplicationBuilder app, IWebHostEnvironment env);
    }
}
