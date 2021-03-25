using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Concrete;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.FileOperations.Concrete;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Data.Repositories;
using Milvasoft.SampleAPI.Data.Utils;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Services.Concrete;
using Milvasoft.SampleAPI.Utils;
using Moq;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.UnitTest.TestHelpers
{
    public class ServiceCollectionHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public ServiceCollectionHelper()
        {
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Development.json")
           .Build();

            _services = new ServiceCollection();

            IJsonOperations jsonOperations = new JsonOperations();

            _services.AddLogging();
            _services.AddLocalization(options => options.ResourcesPath = "Resources");

            ConfigureDatabase();
            ConfigureDependencyInjection();
            ConfigureIdentity();
            ConfigureJWT(jsonOperations);

            var app = _services.BuildServiceProvider().GetRequiredService<IApplicationBuilder>();
            app.ResetDatabaseAsync().Wait();
        }

        /// <summary>
        /// Configures database connection for test.
        /// </summary>
        private void ConfigureDatabase()
        {
            var connectionString = _configuration.GetConnectionString("PostgreConnection");

            _services.AddEntityFrameworkNpgsql().AddDbContext<EducationAppDbContext>(opts =>
            {
                opts.UseNpgsql(connectionString, b => b.MigrationsAssembly("Milvasoft.SampleAPI.Data").EnableRetryOnFailure()).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }).AddSingleton<IAuditConfiguration>(new AuditConfiguration(true, true, true, true, true, true));
        }


        /// <summary>
        /// Configures DI for test.
        /// </summary>
        private void ConfigureDependencyInjection()
        {
            #region Data Access

            _services.AddScoped<IContextRepository<EducationAppDbContext>, ContextRepository<EducationAppDbContext>>();

            _services.AddScoped(typeof(IBaseRepository<,,>), typeof(EducationRepositoryBase<,,>));

            #endregion

            #region Services

            _services.AddScoped<IAccountService, AccountService>();

            #endregion

            _services.AddSingleton<SharedResource>();
            _services.AddScoped<IApplicationBuilder, ApplicationBuilder>();
        }


        /// <summary>
        /// Configures AspNetCore.Identity for test.
        /// </summary>
        private void ConfigureIdentity()
        {
            Action<IdentityOptions> identityOptions = setupAction =>
            {
                //Kullanıcı locklama süresi
                setupAction.Lockout.DefaultLockoutTimeSpan = new TimeSpan(3, 1, 0);//buradaki 3 saaat ekleme veri tabanı saati yanlış olduğundan dolayı // 1 ise 1 dakka kitleniyor
                setupAction.Lockout.MaxFailedAccessAttempts = 5;//Başarısız deneme sayısı
                setupAction.User.RequireUniqueEmail = true;
                setupAction.Password.RequireDigit = false;
                setupAction.Password.RequiredLength = 1;
                setupAction.Password.RequireLowercase = false;
                setupAction.Password.RequireNonAlphanumeric = false;
                setupAction.Password.RequireUppercase = false;
                setupAction.User.AllowedUserNameCharacters = "abcçdefghiıjklmnoöpqrsştuüvwxyzABCÇDEFGHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._";
            };

            _services.AddIdentity<AppUser, AppRole>(identityOptions)
                    .AddEntityFrameworkStores<EducationAppDbContext>()
                    .AddUserValidator<MilvaUserValidation<AppUser, Guid, IStringLocalizer<SharedResource>>>()
                    .AddErrorDescriber<MilvaIdentityDescriber<IStringLocalizer<SharedResource>>>()
                    .AddUserManager<UserManager<AppUser>>()
                    .AddDefaultTokenProviders();


            //_services.ConfigureJWT(jsonOperations);
        }


        /// <summary>
        /// Configures JWT for test.
        /// </summary>
        /// <param name="jSONFile"></param>
        private void ConfigureJWT(IJsonOperations jSONFile)
        {
            var localizer = _services.BuildServiceProvider().GetRequiredService<IStringLocalizer<SharedResource>>();

            var tokenManagement = jSONFile.GetRequiredSingleContentCryptedFromJsonFileAsync<TokenManagement>(Path.Combine(FakeGlobalConstants.RootPath, "StaticFiles", "JSON", "tokenmanagement.json"),
                                                                                                             FakeGlobalConstants.MilvaKey,
                                                                                                             new CultureInfo("tr-TR")).Result;

            _services.AddSingleton(tokenManagement);

            //var tokenOptions = configuration.GetSection("tokenManagement").Get<TokenManagement>();

            _services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents()
                {

                    //Token içinde name kontrol etme
                    OnTokenValidated = (context) =>
                    {
                        var accessToken = context.SecurityToken as JwtSecurityToken;
                        if (string.IsNullOrEmpty(context.Principal.Identity.Name)
                            || accessToken is null)
                            context.Fail(localizer["Unauthorized"]);


                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        ExceptionResponse validationResponse = new ExceptionResponse()
                        {
                            Message = localizer["Forbidden"],
                            Success = false,
                            StatusCode = MilvaStatusCodes.Status403Forbidden
                        };

                        if (context.HttpContext.Response.StatusCode == MilvaStatusCodes.Status401Unauthorized)
                        {
                            // Skip the default logic.
                            context.HandleResponse();

                            context.HttpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
                            context.HttpContext.Response.ContentType = "application/json";

                            if (context.HttpContext.Items.ContainsKey("Token-Expired"))
                            {
                                validationResponse.Message = localizer["AccountTimeOut"];
                                validationResponse.Success = false;
                                validationResponse.StatusCode = MilvaStatusCodes.Status401Unauthorized;

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(validationResponse));
                            }
                            else
                            {
                                validationResponse.Message = localizer["Unauthorized"];
                                validationResponse.Success = false;
                                validationResponse.StatusCode = MilvaStatusCodes.Status401Unauthorized;

                                return context.Response.WriteAsync(JsonConvert.SerializeObject(validationResponse));
                            }
                        }
                        else if (context.HttpContext.Response.StatusCode == MilvaStatusCodes.Status403Forbidden)
                        {
                            // Skip the default logic.
                            context.HandleResponse();

                            context.HttpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
                            context.HttpContext.Response.ContentType = "application/json";

                            validationResponse.Message = localizer["Forbidden"];
                            validationResponse.Success = false;
                            validationResponse.StatusCode = MilvaStatusCodes.Status403Forbidden;

                            return context.Response.WriteAsync(JsonConvert.SerializeObject(validationResponse));
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        //TODO daha önceden adamın tokenı vardı başka biri giriş yapıp yeni token aldı. Yani adamın tokenı artık whitelistte yok. 
                        //Yani yapılacak kontrol şu : giriş yapılmak isterken fail olduysa eğer yani buraya girdiyse, girenin username'i whitelistte 
                        //varsa fakat gönderdiği token whitelisttekinden farklıysa demekki yerine başkası girmiş. Hesabınıza başka bir yerden oturum açıldı hatası verilecek.
                        //Token ve user hederdan alınacak. SingedInUsers listesi kontrol edilecek ve gerekli işlemler yapılacak.
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.HttpContext.Items.Add("Token-Expired", "true");
                        }
                        context.Response.StatusCode = MilvaStatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenManagement.Secret)),
                    ValidIssuer = tokenManagement.Issuer,
                    ValidAudience = tokenManagement.Audience,
                    //ClockSkew = TimeSpan.Zero // remove delay of token when expire

                };
            });

            /* Token ile ilgili
                 issuer: kısacası tokenin oluşturulmasıyla ilintili servis.
                 audience: tokenin kullanılacağı servis.
                 exp: tokenin son valid olduğu tarih.
                 nbf: tokenin aktif olmaya başlayacağı tarih.
                 iat: tokenin oluşturulduğu tarih             
          */
        }

        #region Public Methods

        /// <summary>
        /// Get the service according to <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetService<TService>() => _services.BuildServiceProvider().GetRequiredService<TService>();

        /// <summary>
        /// Get ServiceProvider for test.
        /// </summary>
        /// <returns></returns>
        public ServiceProvider GetServiceProvider() => _services.BuildServiceProvider();

        /// <summary>
        /// Creates a fake instance for <b> IHttpContextAccessor </b>.
        /// 
        /// <para> Remarks; </para>
        /// 
        /// <para> Send the username of the user you want to login. See <paramref name="userName"/> </para>
        /// 
        /// <para> You have to use this method for HttpContext.User.Identity.Name </para>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        public void MockLoggedUserWithHttpContextAccessor(string userName = "MilvaSoft")
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(o => o.HttpContext.User.Identity.Name).Returns(userName);

            _services.AddSingleton((_) => mockHttpContextAccessor.Object);
        }

        #endregion
    }
}
