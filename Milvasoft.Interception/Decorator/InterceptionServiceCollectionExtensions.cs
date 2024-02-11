using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvasoft.Interception.Interceptors;
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
    public static IServiceCollection AddMilvaInterception(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var types = assembly.FindDecorableTypes();

        services.AddScoped<IInterceptorRunner, InterceptorRunner>();
        services.Intercept(typeof(IInterceptorRunner));

        foreach (var type in types)
            services.Intercept(type);

        return services;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static IServiceCollection Intercept<T>(this IServiceCollection services) where T : class
    {
        services.AddScoped<IInterceptorRunner, InterceptorRunner>();
        services.Intercept(typeof(IInterceptorRunner));
        services.Intercept(typeof(T));

        return services;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static IServiceCollection Intercept(this IServiceCollection services, Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            case ServiceLifetime.Scoped:
                services.TryAddScoped(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            case ServiceLifetime.Transient:
                services.TryAddTransient(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
            default:
                services.TryAddScoped(x => new Decorator((type) => (IMilvaInterceptor)x.GetRequiredService(type)));
                break;
        }

        var descriptors = services.Where(x => x.ServiceType == type).ToArray();

        if (descriptors.Length == 0)
        {
            throw new ArgumentException($"Cannot find the service of type '{type}' to decorate. Add '{type}' to service collection before trying to decorate it.");
        }

        foreach (var descriptor in descriptors)
        {
            var index = services.IndexOf(descriptor);

            services.Insert(index, Intercept(descriptor));
            services.RemoveAt(index + 1);
        }

        return services;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static IServiceCollection AddLoggingInterceptor(this IServiceCollection services, Action<ILogInterceptionOptions> interceptionOptions = null)
    {
        if (!services.Any(s => s.ServiceType == typeof(LogInterceptor)))
            services.AddTransient<LogInterceptor>();

        var config = new LogInterceptionOptions();

        interceptionOptions?.Invoke(config);

        services.AddSingleton<ILogInterceptionOptions>(config);

        return services;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static IServiceCollection AddResponseInterceptor(this IServiceCollection services, Action<IResponseInterceptionOptions> interceptionOptions = null)
    {
        if (!services.Any(s => s.ServiceType == typeof(ResponseInterceptor)))
            services.AddTransient<ResponseInterceptor>();

        var config = new ResponseInterceptionOptions();

        interceptionOptions?.Invoke(config);

        services.AddSingleton<IResponseInterceptionOptions>(config);

        return services;
    }

    /// <summary>
    /// Decorates the specified service type descriptor inside <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">Service type to be decorated</typeparam>
    public static IServiceCollection AddCacheInterceptor(this IServiceCollection services, Action<ICacheInterceptionOptions> interceptionOptions = null)
    {
        if (!services.Any(s => s.ServiceType == typeof(CacheInterceptor)))
            services.AddTransient<CacheInterceptor>();

        var config = new CacheInterceptionOptions();

        interceptionOptions?.Invoke(config);

        services.AddSingleton<ICacheInterceptionOptions>(config);

        return services;
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
                            .Where(type => type is { IsClass: false, IsAbstract: true })
                            .Where(type => type != typeof(IInterceptable) && typeof(IInterceptable).IsAssignableFrom(type));

        return types;
    }
}
