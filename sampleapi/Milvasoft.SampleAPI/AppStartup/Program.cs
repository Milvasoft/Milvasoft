using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Configures web api configurations.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
             WebHost.CreateDefaultBuilder(args)
                  .UseUrls(/*"https://0.0.0.0:5001",*/"http://0.0.0.0:5000")
                  .UseWebRoot("wwwroot")
                  .UseStartup<Startup>()
                  .UseDefaultServiceProvider(options => options.ValidateScopes = false);
    }
}
