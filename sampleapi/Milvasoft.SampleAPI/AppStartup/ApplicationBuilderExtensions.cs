using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds swagger documentation to pipeline.
        /// </summary>
        /// <param name="app"></param>
        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.DefaultModelExpandDepth(-1);
                   c.DefaultModelsExpandDepth(1);
                   c.DefaultModelRendering(ModelRendering.Model);
                   c.DocExpansion(DocExpansion.None);
                   c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Milva Sample App v1");
                   c.InjectStylesheet("/swagger-ui/custom.css");
                   c.InjectJavascript("/swagger-ui/custom.js");
               });

        }

        /// <summary>
        /// <para><b>EN: </b>Adds the required middleware to use the localization. Configures the options before add.</para>
        /// <para><b>TR: </b>Lokalizasyon için gereken middleware ekler. Eklemeden önce opsiyonları konfigüre eder.</para>
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder ConfigureLocalization(this IApplicationBuilder app)
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("tr-TR"),
                new CultureInfo("en-US")
            };
            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("tr-TR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            return app.UseRequestLocalization(options);
        }

    }
}
