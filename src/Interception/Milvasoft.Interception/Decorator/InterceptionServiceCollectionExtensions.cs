using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Milvasoft.Core.Utils.Converters;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Interception.Interceptors.Response;
using Milvasoft.Interception.Interceptors.Runner;
using System.Reflection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Provides extension methods for registering <see cref="Decorator"/> and decorated types.
/// </summary>
public static class InterceptionServiceCollectionExtensions
{
    /// <summary>
    /// Decorates service collection with intercaptable service methods. 
    /// Use this method after all registrations. Because it can be decorates <see cref="IInterceptable"/> services from service collection.
    /// 
    /// <para> You can decorate the service collection with <see cref="Intercept(InterceptionBuilder, Type, ServiceLifetime)"/> overload. </para>
    /// 
    /// <para><paramref name="assembly"/> is assembly containing classes that contain the methods to be intercepted </para>
    /// </summary>
    /// <param name="services"> Service collection to be decorated. </param>
    /// <param name="assembly"> An assembly containing classes that contain the methods to be intercepted. </param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static InterceptionBuilder AddMilvaInterception(this IServiceCollection services, Assembly assembly, IConfigurationManager configurationManager = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var types = assembly.FindDecorableTypes()?.ToList();

        return services.AddMilvaInterception(types: types, configurationManager);
    }

    /// <summary>
    /// Decorates service collection with intercaptable service methods. 
    /// Use this method after all registrations. Because it can be decorates <see cref="IInterceptable"/> services from service collection.
    /// 
    /// <para> You can decorate the service collection with <see cref="Intercept(InterceptionBuilder, Type, ServiceLifetime)"/> overload. </para>
    /// 
    /// <para><paramref name="types"/> Types that contain the methods to be intercepted </para>
    /// </summary>
    /// <param name="services"> Service collection to be decorated. </param>
    /// <param name="types"> Types that contain the methods to be intercepted. </param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static InterceptionBuilder AddMilvaInterception(this IServiceCollection services, List<Type> types, IConfigurationManager configurationManager = null)
    {
        var builder = new InterceptionBuilder(services, configurationManager);

        services.ConfigureCurrentMilvaJsonSerializerOptions();

        var externalTypes = builder.Services?.Where(i => i.ImplementationType?.GetInterfaces() != null && Array.Exists(i.ImplementationType.GetInterfaces(), t => t == typeof(IInterceptable))).Select(i => i.ServiceType);

        types = types.Concat(externalTypes).Distinct().ToList();

        builder.WithDefaultInterceptorRunner();

        foreach (var type in types)
            builder.Intercept(type);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TInterceptor">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithInterceptor<TInterceptor>(this InterceptionBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TInterceptor : class, IMilvaInterceptor
    {
        if (!builder.Services.Any(s => s.ServiceType == typeof(TInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(TInterceptor), typeof(TInterceptor), serviceLifetime));

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TInterceptor">Service type to be decorated</typeparam>
    /// <typeparam name="TInterceptorOptions"></typeparam>
    /// <typeparam name="TInterceptorOptionsInterface"></typeparam>
    public static InterceptionBuilder WithInterceptor<TInterceptor, TInterceptorOptionsInterface, TInterceptorOptions>(this InterceptionBuilder builder, Action<TInterceptorOptionsInterface> interceptionOptions)
        where TInterceptor : class, IMilvaInterceptor
        where TInterceptorOptions : class, TInterceptorOptionsInterface, new()
        where TInterceptorOptionsInterface : IInterceptionOptions
    {
        var config = new TInterceptorOptions();

        interceptionOptions?.Invoke(config);

        builder.Services.AddSingleton(typeof(TInterceptorOptionsInterface), config);

        return builder.WithInterceptor<TInterceptor>(config.InterceptorLifetime);
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder Intercept<T>(this InterceptionBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        => builder.Intercept(typeof(T), serviceLifetime);

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder Intercept(this InterceptionBuilder builder, Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(Decorator), x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)), serviceLifetime));

        var descriptors = builder.Services.Where(x => x.ServiceType == type).ToArray();

        if (descriptors.Length == 0)
            throw new ArgumentException($"Cannot find the service of type '{type}' to decorate. Add '{type}' to service collection before trying to decorate it.");

        foreach (var descriptor in descriptors)
        {
            var index = builder.Services.IndexOf(descriptor);

            builder.Services.Insert(index, Intercept(descriptor));
            builder.Services.RemoveAt(index + 1);
        }

        return builder;
    }

    #region InterceptorRunner

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/> with a <see cref="InterceptorRunner"/>.
    /// </summary>
    /// <param name="builder">The interception builder.</param>
    /// <returns>The interception builder.</returns>
    public static InterceptionBuilder WithDefaultInterceptorRunner(this InterceptionBuilder builder)
    {
        builder.Services.AddScoped<IInterceptorRunner, InterceptorRunner>();
        builder.Intercept(typeof(IInterceptorRunner));

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/> with a custom interceptor runner.
    /// </summary>
    /// <typeparam name="TInterceptorRunner">The type of the custom interceptor runner.</typeparam>
    /// <param name="builder">The interception builder.</param>
    /// <returns>The interception builder.</returns>
    public static InterceptionBuilder WithInterceptorRunner<TInterceptorRunner>(this InterceptionBuilder builder) where TInterceptorRunner : class, IInterceptorRunner
    {
        builder.Services.RemoveAll(typeof(IInterceptorRunner));

        builder.Services.AddScoped<IInterceptorRunner, TInterceptorRunner>();
        builder.Intercept(typeof(IInterceptorRunner));

        return builder;
    }

    #endregion

    #region Log

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithLogInterceptor(this InterceptionBuilder builder, Action<ILogInterceptionOptions> interceptionOptions)
    {
        var config = new LogInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(LogInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(LogInterceptor), typeof(LogInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<ILogInterceptionOptions>(config);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithLogInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithLogInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(LogInterceptionOptions.SectionName);

        builder.Services.AddOptions<LogInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = (ILogInterceptionOptions)section.Get<LogInterceptionOptions>();

        builder.WithLogInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.ExtraLoggingPropertiesSelector = options.ExtraLoggingPropertiesSelector;
            opt.LogDefaultParameters = options.LogDefaultParameters;
            opt.UseUtcForLogTimes = options.UseUtcForLogTimes;
            opt.ExcludeResponseMetadataFromLog = options.ExcludeResponseMetadataFromLog;
            opt.AsyncLogging = options.AsyncLogging;
        });

        return builder;
    }

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureLogInterceptionOptions(this InterceptionBuilder builder, Action<LogInterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<LogInterceptionOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new LogInterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<ILogInterceptionOptions>(opt =>
        {
            opt.ExtraLoggingPropertiesSelector = config.ExtraLoggingPropertiesSelector ?? opt.ExtraLoggingPropertiesSelector;
        });

        builder.Services.PostConfigure<LogInterceptionOptions>(opt =>
        {
            opt.ExtraLoggingPropertiesSelector = config.ExtraLoggingPropertiesSelector ?? opt.ExtraLoggingPropertiesSelector;
        });

        return builder;
    }

    #endregion

    #region Response 

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithResponseInterceptor(this InterceptionBuilder builder, Action<IResponseInterceptionOptions> interceptionOptions)
    {
        var config = new ResponseInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(ResponseInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(ResponseInterceptor), typeof(ResponseInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<IResponseInterceptionOptions>(config);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithResponseInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithResponseInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(ResponseInterceptionOptions.SectionName);

        builder.Services.AddOptions<ResponseInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<ResponseInterceptionOptions>();

        builder.WithResponseInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.TranslateMetadata = options.TranslateMetadata;
            opt.ApplyMetadataRules = options.ApplyMetadataRules;
            opt.MetadataCreationEnabled = options.MetadataCreationEnabled;
            opt.TranslateResultMessages = options.TranslateResultMessages;
            opt.ApplyLocalizationFunc = options.ApplyLocalizationFunc;
            opt.HideByRoleFunc = options.HideByRoleFunc;
            opt.MaskByRoleFunc = options.MaskByRoleFunc;
        });

        return builder;
    }

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureResponseInterceptionOptions(this InterceptionBuilder builder, Action<ResponseInterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<LogInterceptionOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new ResponseInterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<IResponseInterceptionOptions>(opt =>
        {
            opt.HideByRoleFunc = config.HideByRoleFunc ?? opt.HideByRoleFunc;
            opt.MaskByRoleFunc = config.MaskByRoleFunc ?? opt.MaskByRoleFunc;
            opt.ApplyLocalizationFunc = config.ApplyLocalizationFunc ?? opt.ApplyLocalizationFunc;
        });

        builder.Services.PostConfigure<ResponseInterceptionOptions>(opt =>
        {
            opt.HideByRoleFunc = config.HideByRoleFunc ?? opt.HideByRoleFunc;
            opt.MaskByRoleFunc = config.MaskByRoleFunc ?? opt.MaskByRoleFunc;
            opt.ApplyLocalizationFunc = config.ApplyLocalizationFunc ?? opt.ApplyLocalizationFunc;
        });

        return builder;
    }

    #endregion

    #region Cache

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithCacheInterceptor(this InterceptionBuilder builder, Action<ICacheInterceptionOptions> interceptionOptions)
    {
        var config = new CacheInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(CacheInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(CacheInterceptor), typeof(CacheInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<ICacheInterceptionOptions>(config);

        if (config.IncludeRequestHeadersWhenCaching && !builder.Services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
            builder.Services.AddHttpContextAccessor();

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithCacheInterceptor(this InterceptionBuilder builder)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithCacheInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(CacheInterceptionOptions.SectionName);

        builder.Services.AddOptions<CacheInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        var options = section.Get<CacheInterceptionOptions>();

        builder.WithCacheInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.CacheAccessorAssemblyQualifiedName = options.CacheAccessorAssemblyQualifiedName;
            opt.CacheAccessorType = options.CacheAccessorType;
            opt.CacheKeyConfigurator = options.CacheKeyConfigurator;
            opt.IgnoredRequestHeaderKeys = options.IgnoredRequestHeaderKeys;
            opt.IncludeRequestHeadersWhenCaching = options.IncludeRequestHeadersWhenCaching;
        });

        return builder;
    }

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureCacheInterceptionOptions(this InterceptionBuilder builder, Action<CacheInterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfigureOptions<CacheInterceptionOptions>)))
            throw new MilvaDeveloperException("Please configure options with WithOptions() builder method before post configuring.");

        var config = new CacheInterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        builder.Services.UpdateSingletonInstance<ICacheInterceptionOptions>(opt =>
        {
            opt.CacheKeyConfigurator = config.CacheKeyConfigurator ?? opt.CacheKeyConfigurator;
            opt.CacheAccessorAssemblyQualifiedName = config.CacheAccessorType?.AssemblyQualifiedName ?? opt.CacheAccessorAssemblyQualifiedName;
        });

        builder.Services.PostConfigure<CacheInterceptionOptions>(opt =>
        {
            opt.CacheKeyConfigurator = config.CacheKeyConfigurator ?? opt.CacheKeyConfigurator;
            opt.CacheAccessorAssemblyQualifiedName = config.CacheAccessorType?.AssemblyQualifiedName ?? opt.CacheAccessorAssemblyQualifiedName;
        });

        return builder;
    }

    #endregion

    #region Activity

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    public static InterceptionBuilder WithActivityInterceptor(this InterceptionBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        if (!builder.Services.Any(s => s.ServiceType == typeof(ActivityInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(ActivityInterceptor), typeof(ActivityInterceptor), serviceLifetime));

        return builder;
    }

    #endregion

    /// <summary>
    /// If options are made from the configuration file, configures options that cannot be made from the configuration file.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="postConfigureAction"></param>
    /// <returns></returns>
    public static InterceptionBuilder PostConfigureInterceptionOptions(this InterceptionBuilder builder, Action<InterceptionPostConfigureOptions> postConfigureAction)
    {
        if (postConfigureAction == null)
            throw new MilvaDeveloperException("Please provide post configure options.");

        var config = new InterceptionPostConfigureOptions();

        postConfigureAction.Invoke(config);

        if (IsObjectModified(config.Log))
            builder.PostConfigureLogInterceptionOptions(opt =>
            {
                opt.ExtraLoggingPropertiesSelector = config.Log.ExtraLoggingPropertiesSelector;
            });

        if (IsObjectModified(config.Response))
            builder.PostConfigureResponseInterceptionOptions(opt =>
            {
                opt.MaskByRoleFunc = config.Response.MaskByRoleFunc;
                opt.HideByRoleFunc = config.Response.HideByRoleFunc;
                opt.ApplyLocalizationFunc = config.Response.ApplyLocalizationFunc;
            });

        if (IsObjectModified(config.Cache))
            builder.PostConfigureCacheInterceptionOptions(opt =>
            {
                opt.CacheAccessorType = config.Cache.CacheAccessorType;
                opt.CacheKeyConfigurator = config.Cache.CacheKeyConfigurator;
            });

        return builder;
    }

    /// <summary>
    /// Decorates the types added to the service collection to intercept them with Castle.Core's ProxyGenerator.
    /// </summary>
    /// <param name="serviceDescriptor"></param>
    /// <returns></returns>
    private static ServiceDescriptor Intercept(ServiceDescriptor serviceDescriptor)
    {
        object DecoratedFactory(IServiceProvider serviceProvider)
        {
            var implementation = serviceProvider.GetInstance(serviceDescriptor);

            var decorator = serviceProvider.GetRequiredService<Decorator>();

            return decorator.For(serviceDescriptor.ServiceType, implementation, serviceProvider);
        }

        return ServiceDescriptor.Describe(serviceType: serviceDescriptor.ServiceType,
                                          implementationFactory: DecoratedFactory,
                                          lifetime: serviceDescriptor.Lifetime);
    }

    /// <summary>
    /// Finds the classes to be intercepted in the assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    private static IEnumerable<Type> FindDecorableTypes(this Assembly assembly)
    {
        var types = assembly.GetExportedTypes()
                            .Where(type => (type is { IsClass: false, IsAbstract: true })
                                           && (type != typeof(IInterceptable) && typeof(IInterceptable).IsAssignableFrom(type)));

        return types;
    }

    /// <summary>
    /// Determines whether object is modified or not.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static bool IsObjectModified(object obj) => Array.Exists(obj.GetType().GetProperties(), i => i.GetValue(obj) != null || i.GetValue(obj) != default);
}
