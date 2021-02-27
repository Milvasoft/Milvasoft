using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Milvasoft.Helpers.Encryption.Concrete;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Data.Abstract;
using Milvasoft.SampleAPI.Data.Concrete;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.Seed;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Services.Concrete;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.IO;
using System.Reflection;

namespace Milvasoft.SampleAPI
{
    /// <summary>
    /// Configuration class of project.
    /// </summary>
    public class Startup
    {
        #region Fields
        private const QueryTrackingBehavior NO_TRACKING = QueryTrackingBehavior.NoTracking;
        #endregion

        /// <summary>
        /// Gets or sets configuration object.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Creates a new <see cref="Startup"/> instances.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opts.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            });

            services.AddDbContext<TodoAppDbContext>(opt =>
            {
                opt.UseMySql(Configuration.GetConnectionString("MySqlConnection"),
                                     new MySqlServerVersion(new Version(8, 0, 21)),
                                     mySqlOptionsAction: b =>
                                     {
                                         b.EnableRetryOnFailure();
                                         b.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                                     }).UseQueryTrackingBehavior(NO_TRACKING);

            });



            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IBaseService<TodoDTO>, TodoService>();
            services.AddScoped<IBaseService<CategoryDTO>, CategoryService>();

            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "Sample App ",
                    Description = "Sample for for Milvasoft.Helpers Library",
                    TermsOfService = new Uri("https://milvasoft.com"),
                    Contact = new OpenApiContact { Name = "Milvasoft Corporation", Email = "info@milvasoft.com", Url = new Uri("https://milvasoft.com") },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                opt.IncludeXmlComments(xmlPath);

            });

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Defult", "{controller=Todo}/{action=GetTodos}/{id?}");
            });

            #region Seed

            var dbContext = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<TodoAppDbContext>();

            dbContext.Database.MigrateAsync().Wait();

            SeedData.ResetDatas().Wait();

            #endregion

            app.UseSwagger(conf =>
            {
                conf.RouteTemplate = "/docs/{documentName}/docs.json";
            }).UseSwaggerUI(conf =>
               {
                   conf.SwaggerEndpoint($"/docs/v1.0/docs.json", "Todo Demo App v1.0");
               });
        }
    }
}
