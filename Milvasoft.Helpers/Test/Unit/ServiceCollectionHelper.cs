using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.FileOperations.Concrete;
using Moq;
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
        /// Constructor of <see cref="ServiceCollectionHelper"/>.
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
        }

        /// <summary>
        /// Constructor of <see cref="ServiceCollectionHelper"/>.
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
        /// <param name="environment"></param>
        /// <param name="webHostEnvironment"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public IWebHostEnvironment MockTestEnvironment(string environment)
        {
            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockWebHostEnvironment.Setup(p => p.EnvironmentName).Returns(environment);

            return mockWebHostEnvironment.Object;
        }

        /// <summary>
        /// Mock for logged user. Mock HttpContext.User.Identity.Name.
        /// </summary>
        /// <param name="userName"></param>
        public void MockLoggedUser(string userName)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(o => o.HttpContext.User.Identity.Name).Returns(userName);

            _services.AddSingleton((_) => mockHttpContextAccessor.Object);
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
        /// <typeparam name="TUserManager"></typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="userManager"></param>
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
