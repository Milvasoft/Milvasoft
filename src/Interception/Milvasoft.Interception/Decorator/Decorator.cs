using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator.Internal;
using Milvasoft.Interception.Interceptors.Response;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Provides decorated instances based on <see cref="DecorateAttribute"/>.
/// </summary>
public sealed class Decorator
{
    private readonly ProxyGenerator _proxyGenerator = SingletonProxyFactory.GetProxyGenerator();

    /// <summary>
    /// Gets or sets the factory method for creating a <see cref="IMilvaInterceptor"/> instance based on the <see cref="Type"/>.
    /// </summary>
    public Func<Type, IMilvaInterceptor> Factory { get; set; }

    /// <summary>
    /// Creates a decorator whose <see cref="Factory"/> resolves interceptors from <paramref name="serviceProvider"/>.
    /// </summary>
    public Decorator(IServiceProvider serviceProvider)
    {
        Factory = (type) => (IMilvaInterceptor)serviceProvider.GetService(type);
    }

    /// <summary>
    /// <para>Creates a decorator with a custom <see cref="Factory"/> implementation.</para>
    /// <para>Most often this is used for creating <see cref="IMilvaInterceptor"/> instances using dependency container.</para>
    /// </summary>
    /// <param name="factory">A factory method taking <see cref="Type"/> and returning an instance of it.</param>
    public Decorator(Func<Type, IMilvaInterceptor> factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Creates a decorated instance for the given interface and object.
    /// </summary>
    /// <typeparam name="TInterface">Interface for a decorated object to implement.</typeparam>
    /// <typeparam name="TImplementation">Class for a decorated object to decorate.</typeparam>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public TInterface For<TInterface, TImplementation>(TImplementation targetObject, IServiceProvider serviceProvider) where TImplementation : TInterface
        => For<TInterface>(targetObject, serviceProvider);

    /// <summary>
    /// Creates a decorated instance for the given object.
    /// </summary>
    /// <typeparam name="TImplementation">Class for a decorated object to decorate.</typeparam>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public TImplementation For<TImplementation>(TImplementation targetObject, IServiceProvider serviceProvider)
        => (TImplementation)For(typeof(TImplementation), targetObject, serviceProvider);

    /// <summary>
    /// Creates a decorated instance for the given object.
    /// </summary>
    /// <param name="type">Type of the target object.</param>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public object For(Type type, object targetObject, IServiceProvider serviceProvider)
        => For(type, targetObject, serviceProvider, forwardDisposal: false);

    /// <summary>
    /// Creates a decorated instance for the given object.
    /// </summary>
    /// <param name="type">Type of the target object.</param>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="forwardDisposal">
    /// When true and <paramref name="targetObject"/> implements <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/>, the interface proxy implements them too,
    /// so a container that disposes the proxy also disposes the target.
    /// </param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    internal object For(Type type, object targetObject, IServiceProvider serviceProvider, bool forwardDisposal)
    {
        ArgumentNullException.ThrowIfNull(targetObject);

        var interceptor = BuildDecoratorInterceptor(targetObject, serviceProvider);

        if (!type.IsInterface)
            return _proxyGenerator.CreateClassProxyWithTarget(type, targetObject, interceptor);

        var additionalInterfaces = forwardDisposal ? GetDisposableInterfaces(type, targetObject) : Type.EmptyTypes;

        return _proxyGenerator.CreateInterfaceProxyWithTargetInterface(type, additionalInterfaces, targetObject, ProxyGenerationOptions.Default, interceptor);
    }

    /// <summary>
    /// Creates the interceptor which runs the <see cref="IMilvaInterceptor"/>s of each decorated method.
    /// Every interceptor type is resolved once per proxy and interceptors are ordered by <see cref="IMilvaInterceptor.InterceptionOrder"/> (stable).
    /// </summary>
    /// <param name="targetObject"></param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <returns></returns>
    private DecoratorInterceptor BuildDecoratorInterceptor(object targetObject, IServiceProvider serviceProvider)
    {
        var decorations = MethodDecoratorMap.Get(targetObject.GetType());

        var resolvedInterceptors = new Dictionary<Type, IMilvaInterceptor>();
        bool? responseInterceptorRegistered = null;

        var decoratorsMap = new Dictionary<MethodInfo, IMilvaInterceptor[]>(decorations.Count);

        foreach (var (method, decoration) in decorations)
        {
            var interceptorTypes = decoration.DecoratorTypes.AsEnumerable();

            if (decoration.ReturnsResponse && !decoration.DecoratorTypes.Contains(typeof(ResponseInterceptor)))
            {
                responseInterceptorRegistered ??= IsRegistered(serviceProvider, typeof(ResponseInterceptor));

                if (responseInterceptorRegistered.Value)
                    interceptorTypes = interceptorTypes.Append(typeof(ResponseInterceptor));
            }

            var interceptors = interceptorTypes.Select(GetInterceptor).OrderBy(i => i.InterceptionOrder).ToArray();

            if (interceptors.Length != 0)
                decoratorsMap[method] = interceptors;
        }

        return new DecoratorInterceptor(new ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]>(decoratorsMap));

        IMilvaInterceptor GetInterceptor(Type type)
        {
            if (!resolvedInterceptors.TryGetValue(type, out var interceptor))
            {
                interceptor = Factory(type) ?? throw new MilvaDeveloperException($"Interceptor '{type.Name}' is not registered. Register it (for example with the With...Interceptor builder methods) before resolving decorated services.");
                resolvedInterceptors[type] = interceptor;
            }

            return interceptor;
        }
    }

    private static bool IsRegistered(IServiceProvider serviceProvider, Type type)
    {
        if (serviceProvider == null)
            return false;

        if (serviceProvider.GetService(typeof(IServiceProviderIsService)) is IServiceProviderIsService isService)
            return isService.IsService(type);

        return serviceProvider.GetService(type) != null;
    }

    private static Type[] GetDisposableInterfaces(Type type, object targetObject)
    {
        var interfaces = new List<Type>(2);

        if (targetObject is IDisposable && !typeof(IDisposable).IsAssignableFrom(type))
            interfaces.Add(typeof(IDisposable));

        if (targetObject is IAsyncDisposable && !typeof(IAsyncDisposable).IsAssignableFrom(type))
            interfaces.Add(typeof(IAsyncDisposable));

        return [.. interfaces];
    }
}
