using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Milvasoft.Helpers;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Concrete;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.Helpers.Mail;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Data.Repositories;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Localization;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Services.Concrete;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Swagger;
using Newtonsoft.Json;
using Opsiyon.API.Helpers.Swagger;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    /// <summary>
    /// Service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        #region Fields

        private const QueryTrackingBehavior NO_TRACKING = QueryTrackingBehavior.NoTracking;

        #endregion

        /// <summary>
        /// <para><b>EN: </b>Adds MVC services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.</para>
        /// <para><b>TR: </b>Belirtilen Microsoft.Extensions.DependencyInjection.IServiceCollection.IServiceCollection öğesine MVC hizmetleri ekler.</para>
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureMVC(this IServiceCollection services)
        {
            services.AddMvc(opt =>
            {
                //opt.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
                opt.SuppressAsyncSuffixInActionNames = false;
                opt.EnableEndpointRouting = true;
            }).AddNewtonsoftJson(opt =>
            {

                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
              .ConfigureApiBehaviorOptions(options =>
              {
                  options.InvalidModelStateResponseFactory = actionContext =>
                  {
                      return CommonHelper.CustomErrorResponse(actionContext);
                  };
              }).AddDataAnnotationsLocalization();
        }

        /// <summary>
        /// Configured cors policies.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("ApiCorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .Build();
                });

            });
        }

        /// <summary>
        /// Configures AspNetCore.Identity.Mongo and JWT.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="jsonOperations"></param>
        public static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration, IJsonOperations jsonOperations)
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

            services.AddIdentity<AppUser, AppRole>(identityOptions)
                    .AddEntityFrameworkStores<EducationAppDbContext>()
                    .AddUserValidator<MilvaUserValidation<AppUser, Guid, IStringLocalizer<SharedResource>>>()
                    .AddErrorDescriber<MilvaIdentityDescriber<IStringLocalizer<SharedResource>>>()
                    .AddUserManager<UserManager<AppUser>>()
                    .AddDefaultTokenProviders();


            services.ConfigureJWT(jsonOperations);

        }

        /// <summary>
        /// Configures DI.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            #region Data Access

            services.AddScoped<IContextRepository<EducationAppDbContext>, ContextRepository<EducationAppDbContext>>();
            services.AddScoped(typeof(IBaseRepository<,,>), typeof(EducationRepositoryBase<,,>));

            #endregion

            #region Services
            services.AddScoped<IMentorService, MentorService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUsefulLinkService, UsefulLinkService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IProfessionService, ProfessionService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IAssignmentService, AssignmentService>();

            #endregion

            services.AddSingleton<IMilvaMailSender>(new MilvaMailSender("education@milvasoft.com",
                                                                        new NetworkCredential("education@milvasoft.com", "I11Blx8i*%hi"),
                                                                        587,
                                                                        "mail.milvasoft.com"));
            services.AddScoped<IMilvaLogger, EducationLogger>();
            services.AddSingleton<SharedResource>();

            services.AddHttpContextAccessor();

            services.AddHttpClient<IAccountService, AccountService>(configureClient: opt =>
            {

                opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                opt.DefaultRequestVersion = new Version(1, 0);
            });
        }

        /// <summary>
        /// Configures database connection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PostgreConnection");

            //services.AddEntityFrameworkNpgsql().AddSingleton<IAuditConfiguration>(new AuditConfiguration(true, true, true, true, true, true));

            services.AddEntityFrameworkNpgsql().AddDbContext<EducationAppDbContext>(opts =>
            {
                opts.UseNpgsql(connectionString, b => b.MigrationsAssembly("Milvasoft.SampleAPI.Data").EnableRetryOnFailure()).UseQueryTrackingBehavior(NO_TRACKING);
            }).AddSingleton<IAuditConfiguration>(new AuditConfiguration(true, true, true, true, true, true));

        }

        /// <summary>
        /// Configures API versioning.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
        }

        /// <summary>
        /// Configures Swagger documentation.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "Milvasoft Education",
                    Description = "Education for Milvasoft Person",
                    TermsOfService = new Uri("https://milvasoft.com"),
                    Contact = new OpenApiContact { Name = "Milvasoft Corporation", Email = "info@milvasoft.com", Url = new Uri("https://milvasoft.com") },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                });

                //options.SchemaFilter<CustomAttributeSchemaFilter>();
                //options.SchemaFilter<SwaggerExcludeFilter>();
                options.DocumentFilter<EntityDocumentFilter>();
                options.OperationFilter<RequestHeaderFilter>();
                options.OperationFilter<CustomAttributeOperationFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();


                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                  new OpenApiSecurityScheme
                  {
                   Reference = new OpenApiReference
                    {
                     Type = ReferenceType.SecurityScheme,
                     Id = "Bearer"
                   }
                  },
                  new string[] {}
                  }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            //services.AddSwaggerGenNewtonsoftSupport();
        }


        #region Private Methods 

        /// <summary>
        /// Configures JWT Token Authentication.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jSONFile"></param>
        private static void ConfigureJWT(this IServiceCollection services, IJsonOperations jSONFile)
        {
            var tokenManagement = jSONFile.GetRequiredSingleContentCryptedFromJsonFileAsync<TokenManagement>(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "tokenmanagement.json"),
                                                                                                             GlobalConstants.MilvaKey,
                                                                                                             new CultureInfo("tr-TR")).Result;

            services.AddSingleton<ITokenManagement>(tokenManagement);

            //var tokenOptions = configuration.GetSection("tokenManagement").Get<TokenManagement>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {

                IStringLocalizer<SharedResource> GetLocalizerInstance(HttpContext httpContext)
                {
                    return httpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
                }

                Task ReturnResponse(HttpContext httpContext, string localizerKey, int statusCode)
                {
                    if (!httpContext.Response.HasStarted)
                    {
                        var localizer = GetLocalizerInstance(httpContext);

                        ExceptionResponse validationResponse = new()
                        {
                            Message = localizer["Unauthorized"],
                            Success = false,
                            StatusCode = statusCode
                        };

                        httpContext.Response.ContentType = "application/json";
                        httpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
                        return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(validationResponse));
                    }
                    return Task.CompletedTask;
                }

                x.Events = new JwtBearerEvents()
                {
                    //Token içinde name kontrol etme
                    OnTokenValidated = (context) =>
                    {
                        if (string.IsNullOrEmpty(context.Principal.Identity.Name)
                            || context.SecurityToken is not JwtSecurityToken accessToken)
                        //|| !SignedInUsers.SignedInUserTokens.ContainsKey(context.Principal.Identity.Name)
                        //|| SignedInUsers.SignedInUserTokens[context.Principal.Identity.Name] != accessToken.RawData)
                        {
                            var localizer = GetLocalizerInstance(context.HttpContext);

                            context.Fail(localizer["Unauthorized"]);
                            ReturnResponse(context.HttpContext, "Unauthorized", MilvaStatusCodes.Status401Unauthorized);
                        }

                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        return ReturnResponse(context.HttpContext, "Forbidden", MilvaStatusCodes.Status403Forbidden);
                    },
                    OnChallenge = context =>
                    {
                        // Skip the default logic.
                        context.HandleResponse();

                        return ReturnResponse(context.HttpContext, "Unauthorized", MilvaStatusCodes.Status401Unauthorized);
                    },
                    OnAuthenticationFailed = context =>
                    {
                        string localizerKey = "Unauthorized";

                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            localizerKey = "TokenExpired";

                        return ReturnResponse(context.HttpContext, localizerKey, MilvaStatusCodes.Status401Unauthorized);
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

        #endregion
    }
}
