using Castle.DynamicProxy;
using Milvasoft.Interception.Decorator.Internal;
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
    /// <para>Creates a decorator with <see cref="Factory"/> set to <see cref="Activator.CreateInstance(Type)"/>.</para>
    /// <para>This only allows to create <see cref="IMilvaInterceptor"/> instances that have a parameterless constructor.</para>
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
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public TInterface For<TInterface, TImplementation>(TImplementation targetObject) where TImplementation : TInterface
        => (TInterface)For(typeof(TInterface), targetObject);

    /// <summary>
    /// Creates a decorated instance for the given object.
    /// </summary>
    /// <typeparam name="TImplementation">Class for a decorated object to decorate.</typeparam>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public TImplementation For<TImplementation>(TImplementation targetObject)
        => (TImplementation)For(typeof(TImplementation), targetObject);

    /// <summary>
    /// Creates a decorated instance for the given object.
    /// </summary>
    /// <param name="type">Type of the target object.</param>
    /// <param name="targetObject">Object to be decorated.</param>
    /// <returns>A dynamic proxy with <see cref="IMilvaInterceptor"/> instances as interceptors.</returns>
    public object For(Type type, object targetObject)
    {
        ArgumentNullException.ThrowIfNull(targetObject);

        var interceptor = BuildDecoratorInterceptor(targetObject);

        var proxy = type.IsInterface
                         ? _proxyGenerator.CreateInterfaceProxyWithTargetInterface(type, targetObject, interceptor)
                         : _proxyGenerator.CreateClassProxyWithTarget(type, targetObject, interceptor);

        return proxy;
    }

    /// <summary>
    /// Creted <see cref="IMilvaInterceptor"/> instances.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="targetObject"></param>
    /// <returns></returns>
    private DecoratorInterceptor BuildDecoratorInterceptor<TImplementation>(TImplementation targetObject)
    {
        var decoratorTypesMap = MethodDecoratorMap.Get(targetObject.GetType());

        var decoratorsMap = decoratorTypesMap.ToDictionary(typeMapItem => typeMapItem.Key,
                                                           typeMapItem => typeMapItem.Value.Select(type => GetDecorator(type)).OrderBy(i => i.InterceptionOrder).ToArray());

        return new DecoratorInterceptor(new ReadOnlyDictionary<MethodInfo, IMilvaInterceptor[]>(decoratorsMap));

        IMilvaInterceptor GetDecorator(Type type)
        {
            return Factory(type);
        }
    }
}
