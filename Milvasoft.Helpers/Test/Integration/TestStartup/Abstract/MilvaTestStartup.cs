using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Milvasoft.Helpers.Test.Integration.TestStartup.Abstract
{
    /// <summary>
    /// Fake startup for integration test.
    /// </summary>
    public abstract class MilvaTestStartup
    {
        /// <summary>
        /// Constructor of <see cref="MilvaTestStartup"/>.
        /// </summary>
        public MilvaTestStartup()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
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
