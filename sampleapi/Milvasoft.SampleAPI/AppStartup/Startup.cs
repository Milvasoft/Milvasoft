using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Caching;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.FileOperations.Concrete;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Extensions;
using Milvasoft.Helpers.MultiTenancy.ResolutionStrategy;
using Milvasoft.Helpers.MultiTenancy.Store;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Middlewares;

namespace Milvasoft.SampleAPI.AppStartup
{
    /// <summary>
    /// Configuration class of project.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets or sets configuration object.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        public static IStringLocalizer<SharedResource> SharedStringLocalizer;

        public static IWebHostEnvironment WebHostEnvironment { get; set; }
        public static IJsonOperations _jsonOperations { get; set; }

        /// <summary>
        /// Creates a new <see cref="Startup"/> instances.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            WebHostEnvironment = env;
            Configuration = configuration;
            _jsonOperations = new JsonOperations();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMultiTenancy<CachedTenant, TenantId>()
                 .WithResolutionStrategy<HeaderResolutionStrategy>()
                 .WithStore<CachedTenantStore<CachedTenant, TenantId>>();

            services.ConfigureMVC();

            var cacheOptions = new RedisCacheServiceOptions("127.0.0.1:6379")
            {
                Lifetime = ServiceLifetime.Scoped,
                ConnectWhenCreatingNewInstance = false
            };

            cacheOptions.ConfigurationOptions.AbortOnConnectFail = false;
            cacheOptions.ConfigurationOptions.ConnectTimeout = 300;
            cacheOptions.ConfigurationOptions.SyncTimeout = 300;
            cacheOptions.ConfigurationOptions.ConnectRetry = 1;

            services.AddMilvaRedisCaching(cacheOptions);

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.ConfigureDependencyInjection();

            services.ConfigureDatabase(Configuration);

            services.ConfigureIdentity(Configuration, _jsonOperations);

            services.ConfigureVersioning();

            services.ConfigureSwagger();

        }

        public static void ConfigureMultitenantContainer(CachedTenant t, ContainerBuilder c)
        {
            c.Register(container =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<EducationAppDbContext>();
                optionsBuilder.UseNpgsql(t.ConnectionString, b => b.MigrationsAssembly("Milvasoft.SampleAPI.Data").EnableRetryOnFailure()).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                return new EducationAppDbContext(optionsBuilder.Options, container.Resolve<IHttpContextAccessor>(), container.Resolve<IAuditConfiguration>());
            }).SingleInstance();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureLocalization();

            app.UseExceptionHandlerMiddleware();

            app.UseMilvaGeneralMiddleware();

            app.UseStaticFiles();

            //app.UseHttpsRedirection();

            app.UseMultiTenancy<CachedTenant, TenantId>().UseMultiTenantContainer<CachedTenant, TenantId>();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Default", "{controller=Todo}/{action=GetTodos}/{id?}");
            });

            app.ConfigureSwagger();


            #region Seed

            //app.ResetDatabaseAsync().Wait();

            #endregion
        }
    }
}
