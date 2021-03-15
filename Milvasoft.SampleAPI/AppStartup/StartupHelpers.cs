using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Concrete;
using Milvasoft.Helpers.DataAccess.MilvaContext;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.FileOperations.Abstract;
using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.Data.Abstract;
using Milvasoft.SampleAPI.Data.Concrete;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.Entity.Identity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Services.Concrete;
using Milvasoft.SampleAPI.Utils;
using Milvasoft.SampleAPI.Utils.Swagger;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.AppStartup
{
    /// <summary>
    /// Helpers for service collection.
    /// </summary>
    public static partial class StartupHelpers
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

            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
              .ConfigureApiBehaviorOptions(options =>
              {
                  options.InvalidModelStateResponseFactory = actionContext =>
                  {
                      return CustomErrorResponse(actionContext);
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

            services.AddIdentity<MilvaUser, MilvaRole>(identityOptions)
                    .AddEntityFrameworkStores<TodoAppDbContext>()
                    .AddUserValidator<MilvaUserValidation<MilvaUser, Guid, IStringLocalizer<SharedResource>>>()
                    .AddErrorDescriber<MilvaIdentityDescriber<IStringLocalizer<SharedResource>>>()
                    .AddUserManager<UserManager<MilvaUser>>()
                    .AddDefaultTokenProviders();


            services.ConfigureJWT(jsonOperations);

        }

        /// <summary>
        /// Configures DI.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<,,>), typeof(BaseRepo<,,>));
            services.AddScoped<IContextRepository<TodoAppDbContext>, ContextRepository<TodoAppDbContext>>();

            #region Services

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IBaseService<TodoDTO>, TodoService>();
            services.AddScoped<IBaseService<CategoryDTO>, CategoryService>();

            #endregion

            services.AddSingleton<SharedResource>();

            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// Configures database connection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TodoAppDbContext>(opt =>
            {
                var connectionString = configuration.GetConnectionString("MySqlConnection");

                opt.UseMySql(connectionString,
                             ServerVersion.AutoDetect(connectionString),
                             mySqlOptionsAction: b =>
                             {
                                 b.EnableRetryOnFailure();
                                 b.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                             }).UseQueryTrackingBehavior(NO_TRACKING);

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
                    Title = "Milvasoft.Helpers Sample API",
                    Description = "Sample API for Milvasoft.Helpers Library",
                    TermsOfService = new Uri("https://milvasoft.com"),
                    Contact = new OpenApiContact { Name = "Milvasoft Corporation", Email = "info@milvasoft.com", Url = new Uri("https://milvasoft.com") },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                });

                //options.SchemaFilter<SwaggerExcludeFilter>();
                //options.DocumentFilter<DocumentFilter>();
                options.OperationFilter<HeaderFilter>();
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
        /// Prepares custom validation model for response.
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private static ObjectResult CustomErrorResponse(ActionContext actionContext)
        {
            var validationErrors = actionContext.ModelState
             .Where(modelError => modelError.Value.Errors.Count > 0)
             .Select(modelError => new ValidationError
             {
                 ValidationFieldName = modelError.Key,
                 ErrorMessageList = modelError.Value.Errors.Select(i => i.ErrorMessage).ToList()
             }).ToList();

            var stringBuilder = new StringBuilder();
            List<string> errorMessageList = new List<string>();
            validationErrors.ForEach(e => e.ErrorMessageList.ForEach(emg => errorMessageList.Add(emg)));
            stringBuilder.AppendJoin(',', errorMessageList);
            var validationResponse = new ExceptionResponse
            {
                Success = false,
                Message = stringBuilder.ToString(),
                StatusCode = MilvaStatusCodes.Status600Exception,
                Result = new object(),
                ErrorCodes = new List<int>()
            };
            actionContext.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvaStatusCodes.Status600Exception));

            return new OkObjectResult(validationResponse);
        }

        /// <summary>
        /// Configures JWT Token Authentication.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="jSONFile"></param>
        private static void ConfigureJWT(this IServiceCollection services, IJsonOperations jSONFile)
        {
            var localizer = services.BuildServiceProvider().GetRequiredService<IStringLocalizer<SharedResource>>();

            var tokenManagement = jSONFile.GetRequiredSingleContentCryptedFromJsonFileAsync<TokenManagement>(Path.Combine(GlobalConstants.RootPath, "StaticFiles", "JSON", "tokenmanagement.json"),
                                                                                                             GlobalConstants.MilvaKey,
                                                                                                             new CultureInfo("tr-TR")).Result;

            services.AddSingleton(tokenManagement);

            //var tokenOptions = configuration.GetSection("tokenManagement").Get<TokenManagement>();

            services.AddAuthentication(x =>
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

        #endregion
    }

    /// <summary>
    /// Helpers for application builder.
    /// </summary>
    public static partial class StartupHelpers
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
