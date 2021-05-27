using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.LifetimeManagement;
using Milvasoft.SampleAPI.DTOs;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateWebHostBuilder(args).Build().RunAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Configures web api configurations.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
                  Host.CreateDefaultBuilder(args)
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseUrls(/*"https://0.0.0.0:5001",*/"http://0.0.0.0:5000")
                                .UseWebRoot("wwwroot")
                                .UseStartup<Startup>()
                                .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                  });

    }
}
