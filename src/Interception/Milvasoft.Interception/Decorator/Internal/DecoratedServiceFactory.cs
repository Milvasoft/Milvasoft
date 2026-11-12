using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Creates the replacement registration of an intercepted service. The replacement resolves the original implementation and wraps it with a proxy.
/// </summary>
/// <param name="originalDescriptor">The registration being decorated.</param>
/// <param name="services">The service collection, used to find interceptor lifetimes for singleton services.</param>
internal sealed class DecoratedServiceFactory(ServiceDescriptor originalDescriptor, IServiceCollection services)
{
    private readonly ServiceDescriptor _originalDescriptor = originalDescriptor;
    private readonly IServiceCollection _services = services;

    /// <summary>
    /// Whether <paramref name="descriptor"/> is already a decorated registration.
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static bool IsDecorated(ServiceDescriptor descriptor)
        => descriptor.IsKeyedService
            ? descriptor.KeyedImplementationFactory?.Target is DecoratedServiceFactory
            : descriptor.ImplementationFactory?.Target is DecoratedServiceFactory;

    /// <summary>
    /// Creates the decorated descriptor with the same service type, key and lifetime as the original one.
    /// </summary>
    /// <returns></returns>
    public ServiceDescriptor CreateDescriptor()
        => _originalDescriptor.IsKeyedService
            ? ServiceDescriptor.DescribeKeyed(_originalDescriptor.ServiceType, _originalDescriptor.ServiceKey, CreateKeyed, _originalDescriptor.Lifetime)
            : ServiceDescriptor.Describe(_originalDescriptor.ServiceType, Create, _originalDescriptor.Lifetime);

    private object Create(IServiceProvider serviceProvider) => CreateProxy(serviceProvider, serviceKey: null);

    private object CreateKeyed(IServiceProvider serviceProvider, object serviceKey) => CreateProxy(serviceProvider, serviceKey);

    private object CreateProxy(IServiceProvider serviceProvider, object serviceKey)
    {
        var implementation = serviceProvider.GetInstance(_originalDescriptor, serviceKey, out var createdByProvider);

        var decorator = new Decorator(ResolveInterceptor);

        // Instances created here are owned by the container, which only sees the proxy; let the proxy forward disposal to them.
        return decorator.For(_originalDescriptor.ServiceType, implementation, serviceProvider, forwardDisposal: createdByProvider);

        IMilvaInterceptor ResolveInterceptor(Type interceptorType)
        {
            // A singleton must not capture a scoped interceptor (it would live in the root scope and see root scoped services);
            // such interceptors are resolved from a new scope on every call instead.
            if (_originalDescriptor.Lifetime == ServiceLifetime.Singleton && IsScoped(interceptorType))
                return new PerCallScopedInterceptor(serviceProvider.GetRequiredService<IServiceScopeFactory>(), interceptorType);

            return (IMilvaInterceptor)serviceProvider.GetRequiredService(interceptorType);
        }
    }

    private bool IsScoped(Type interceptorType)
        => _services.LastOrDefault(d => !d.IsKeyedService && d.ServiceType == interceptorType)?.Lifetime == ServiceLifetime.Scoped;
}

/// <summary>
/// Runs a scoped interceptor of a singleton service in a new service scope per call.
/// </summary>
internal sealed class PerCallScopedInterceptor : IMilvaInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Type _interceptorType;

    public int InterceptionOrder { get; set; }

    public PerCallScopedInterceptor(IServiceScopeFactory scopeFactory, Type interceptorType)
    {
        _scopeFactory = scopeFactory;
        _interceptorType = interceptorType;

        using var scope = scopeFactory.CreateScope();

        InterceptionOrder = ((IMilvaInterceptor)scope.ServiceProvider.GetRequiredService(interceptorType)).InterceptionOrder;
    }

    public async Task OnInvoke(Call call)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var interceptor = (IMilvaInterceptor)scope.ServiceProvider.GetRequiredService(_interceptorType);

        await interceptor.OnInvoke(call);
    }
}
