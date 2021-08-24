using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.FileOperations.Concrete;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Test.Unit
{
    /// <summary>
    /// Includes helper and required methods for unit testing.
    /// </summary>
    public abstract class ServiceCollectionHelper<TUser> where TUser : class
    {
        private readonly IServiceCollection _services;
        private readonly IJsonOperations _jsonOperations;

        /// <summary>
        /// Constructor of <see cref="ServiceCollectionHelper{TUser}"/>.
        /// 
        /// <para> If you use this constructor, <see cref="IServiceCollection"/> and <see cref="IJsonOperations"/> objects are created as a singleton. </para>
        /// 
        /// </summary>
        public ServiceCollectionHelper()
        {
            if (_services == null)
                _services = new ServiceCollection();

            if (_jsonOperations == null)
                _jsonOperations = new JsonOperations();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        /// <summary>
        /// Constructor of <see cref="ServiceCollectionHelper{TUser}"/>.
        /// 
        /// <para> If you use this constructor, you have to send <paramref name="services"/> and <paramref name="jsonOperations"/> objects. </para>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jsonOperations"></param>
        public ServiceCollectionHelper(IServiceCollection services, IJsonOperations jsonOperations)
        {
            _services = services;
            _jsonOperations = jsonOperations;
        }

        #region Abstract Methods

        /// <summary>
        /// Using this method, you can configure the database.
        /// </summary>
        /// <param name="jsonOperations"></param>
        /// <param name="configuration"></param>
        protected abstract void ConfigureDatabase(IJsonOperations jsonOperations = null, IConfiguration configuration = null);

        /// <summary>
        /// You can configure your dependencies by using this method.
        /// </summary>
        protected abstract void ConfigureDependencyInjections();

        /// <summary>
        /// You can configure your indetity operations by using this method.
        /// </summary>
        protected abstract void ConfigureIdentity();

        /// <summary>
        /// You can reset the database for data consistency using this method before each unit test runs.
        /// </summary>
        /// <returns></returns>
        public abstract Task ResetDatabaseAsync();

        #endregion

        #region Methods

        /// <summary>
        /// Creates a fake <see cref="IWebHostEnvironment"/> for the test environment.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="applicationName"></param>
        /// <param name="webRootPath"></param>
        /// <param name="webRootFileProvider"></param>
        /// <param name="contentRootPath"></param>
        /// <param name="contentRootFileProvider"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public IWebHostEnvironment MockTestEnvironment(string environmentName,
                                                       string applicationName = null,
                                                       string webRootPath = null,
                                                       IFileProvider webRootFileProvider = null,
                                                       string contentRootPath = null,
                                                       IFileProvider contentRootFileProvider = null)
        {
            return new TestHostEnvironment
            {
                EnvironmentName = environmentName,
                ApplicationName = applicationName,
                ContentRootFileProvider = contentRootFileProvider,
                ContentRootPath = contentRootPath,
                WebRootFileProvider = webRootFileProvider,
                WebRootPath = webRootPath
            };
        }

        /// <summary>
        /// Mock for logged user. Mock HttpContext.User.Identity.Name.
        /// </summary>
        /// <param name="userName"></param>
        public void MockLoggedUser(string userName)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, userName)
            }, "mock"));

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            _services.AddSingleton((_) => httpContextAccessor);
        }

        /// <summary>
        /// Returns <see cref="ServiceProvider"/>.
        /// </summary>
        /// <returns></returns>
        public ServiceProvider GetServiceProvider() => _services.BuildServiceProvider();

        /// <summary>
        /// Returns service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetService<TService>() => _services.BuildServiceProvider().GetRequiredService<TService>();

        /// <summary>
        /// Returns <see cref="IServiceCollection"/> instance.
        /// </summary>
        /// <returns></returns>
        public IServiceCollection GetServiceCollection() => _services;

        /// <summary>
        /// Returns <see cref="IJsonOperations"/> instance.
        /// </summary>
        /// <returns></returns>
        public IJsonOperations GetJsonOperations() => _jsonOperations;

        /// <summary>
        /// If you want to login with another user, you can use this method.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public TService GetServiceWithAnotherUser<TService>(string userName)
        {
            MockLoggedUser(userName);
            return GetService<TService>();
        }

        /// <summary>
        /// Returns logged user informations.
        /// </summary>
        /// <returns></returns>
        public async Task<TUser> GetLoggedUserInformationsAsync()
        {
            var httpContext = GetServiceProvider().GetRequiredService<IHttpContextAccessor>().HttpContext;

            var userManager = GetServiceProvider().GetRequiredService<UserManager<TUser>>();

            return await userManager.FindByNameAsync(httpContext.User.Identity.Name).ConfigureAwait(false);
        }

        #endregion
    }
}
