using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Interceptors;
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
    /// 
    /// <para> You can decorate the service collection with <see cref="Intercept{T}(IServiceCollection)"/> overload. </para>
    /// 
    /// <para><paramref name="assembly"/> is assembly containing classes that contain the methods to be intercepted </para>
    /// </summary>
    /// <param name="services"> Service collection to be decorated. </param>
    /// <param name="assembly"> An assembly containing classes that contain the methods to be intercepted. </param>
    /// <returns></returns>
    public static InterceptionBuilder AddMilvaInterception(this IServiceCollection services, Assembly assembly, IConfigurationManager configurationManager = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var types = assembly.FindDecorableTypes();

        var builder = new InterceptionBuilder(services, configurationManager);

        var externalTypes = builder.Services.Where(i => i?.ImplementationType?.GetInterfaces()?.Any(i => i == typeof(IInterceptable)) ?? false)?.Select(i => i.ServiceType);

        types = (types?.Concat(externalTypes) ?? externalTypes)?.Distinct()?.ToList();

        builder.Services.AddScoped<IInterceptorRunner, InterceptorRunner>();
        builder.Intercept(typeof(IInterceptorRunner));

        foreach (var type in types)
            builder.Intercept(type);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithInterceptor<T>(this InterceptionBuilder builder) where T : class
    {
        builder.Services.AddScoped<IInterceptorRunner, InterceptorRunner>();
        builder.Intercept(typeof(IInterceptorRunner));
        builder.Intercept(typeof(T));

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder Intercept(this InterceptionBuilder builder, Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                builder.Services.TryAddSingleton(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            case ServiceLifetime.Scoped:
                builder.Services.TryAddScoped(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            case ServiceLifetime.Transient:
                builder.Services.TryAddTransient(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            default:
                builder.Services.TryAddScoped(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
        }

        var descriptors = builder.Services.Where(x => x.ServiceType == type).ToArray();

        if (descriptors.Length == 0)
        {
            throw new ArgumentException($"Cannot find the service of type '{type}' to decorate. Add '{type}' to service collection before trying to decorate it.");
        }

        foreach (var descriptor in descriptors)
        {
            var index = builder.Services.IndexOf(descriptor);

            builder.Services.Insert(index, Intercept(descriptor));
            builder.Services.RemoveAt(index + 1);
        }

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
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
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithLogInterceptor(this InterceptionBuilder builder, Func<IServiceProvider, object> extraLoggingPropertiesSelector = null)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithLogInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(LogInterceptionOptions.SectionName);

        builder.Services.AddOptions<LogInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        builder.Services.PostConfigure<LogInterceptionOptions>(opt =>
        {
            opt.ExtraLoggingPropertiesSelector = extraLoggingPropertiesSelector ?? opt.ExtraLoggingPropertiesSelector;
        });

        var options = (ILogInterceptionOptions)section.Get<LogInterceptionOptions>();

        options.ExtraLoggingPropertiesSelector = extraLoggingPropertiesSelector ?? options.ExtraLoggingPropertiesSelector;

        builder.WithLogInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.ExtraLoggingPropertiesSelector = options.ExtraLoggingPropertiesSelector;
            opt.LogDefaultParameters = options.LogDefaultParameters;
            opt.ExcludeResponseMetadataFromLog = options.ExcludeResponseMetadataFromLog;
            opt.AsyncLogging = options.AsyncLogging;
        });

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
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
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithResponseInterceptor(this InterceptionBuilder builder,
                                                              Func<HideByRoleAttribute, bool> hideByRoleFunc = null,
                                                              Func<string, IMilvaLocalizer, Type, string, string> applyLocalizationFunc = null)
    {
        if (builder.ConfigurationManager == null)
            return builder.WithResponseInterceptor(interceptionOptions: null);

        var section = builder.ConfigurationManager.GetSection(ResponseInterceptionOptions.SectionName);

        builder.Services.AddOptions<ResponseInterceptionOptions>()
                        .Bind(section)
                        .ValidateDataAnnotations();

        builder.Services.PostConfigure<ResponseInterceptionOptions>(opt =>
        {
            opt.HideByRoleFunc = hideByRoleFunc ?? opt.HideByRoleFunc;
            opt.ApplyLocalizationFunc = applyLocalizationFunc ?? opt.ApplyLocalizationFunc;
        });

        var options = section.Get<ResponseInterceptionOptions>();

        options.HideByRoleFunc = hideByRoleFunc ?? options.HideByRoleFunc;
        options.ApplyLocalizationFunc = applyLocalizationFunc ?? options.ApplyLocalizationFunc;

        builder.WithResponseInterceptor(interceptionOptions: (opt) =>
        {
            opt.InterceptorLifetime = options.InterceptorLifetime;
            opt.TranslateMetadata = options.TranslateMetadata;
            opt.TranslateResultMessages = options.TranslateResultMessages;
            opt.ApplyLocalizationFunc = options.ApplyLocalizationFunc;
            opt.HideByRoleFunc = options.HideByRoleFunc;
        });

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithCacheInterceptor(this InterceptionBuilder builder, Action<ICacheInterceptionOptions> interceptionOptions)
    {
        var config = new CacheInterceptionOptions();

        interceptionOptions?.Invoke(config);

        if (!builder.Services.Any(s => s.ServiceType == typeof(CacheInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(CacheInterceptor), typeof(CacheInterceptor), config.InterceptorLifetime));

        builder.Services.AddSingleton<ICacheInterceptionOptions>(config);

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
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
        });

        return builder;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static InterceptionBuilder WithActivityInterceptor(this InterceptionBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        if (!builder.Services.Any(s => s.ServiceType == typeof(ActivityInterceptor)))
            builder.Services.Add(ServiceDescriptor.Describe(typeof(ActivityInterceptor), typeof(ActivityInterceptor), serviceLifetime));

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

            return decorator.For(serviceDescriptor.ServiceType, implementation);
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
}
