using System.Collections.Concurrent;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Finds the <see cref="DecorateAttribute"/> that caused an interceptor to run for a method.
/// The search order mirrors <see cref="MethodDecoratorMap"/> so an attribute that made an interceptor run is always found:
/// implementation method, called (interface) method, interface methods mapped to the implementation, implementation type, called method's declaring type.
/// Results are cached per method and attribute type.
/// </summary>
internal static class InterceptorAttributeLocator
{
    private static readonly ConcurrentDictionary<(MethodInfo Implementation, MethodInfo Called, Type AttributeType), DecorateAttribute> _cache = new();

    /// <summary>
    /// Finds the attribute of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="implementation">The implementation method.</param>
    /// <param name="called">The method at the call site (interface method for interface proxies).</param>
    /// <returns>The found attribute or null.</returns>
    public static T Find<T>(MethodInfo implementation, MethodInfo called) where T : DecorateAttribute
        => (T)_cache.GetOrAdd((implementation, called, typeof(T)), static key => Locate(key.Implementation, key.Called, key.AttributeType));

    private static DecorateAttribute Locate(MethodInfo implementation, MethodInfo called, Type attributeType)
    {
        if (implementation != null)
        {
            if (FindOn(implementation, attributeType) is DecorateAttribute onImplementation)
                return onImplementation;
        }

        if (called != null && called != implementation && FindOn(called, attributeType) is DecorateAttribute onCalled)
            return onCalled;

        var interfaceMethod = implementation?.GetInterfaceMethod();

        if (interfaceMethod != null && FindOn(interfaceMethod, attributeType) is DecorateAttribute onInterfaceMethod)
            return onInterfaceMethod;

        if (implementation?.DeclaringType != null && FindOn(implementation.DeclaringType, attributeType) is DecorateAttribute onImplementationType)
            return onImplementationType;

        if (called?.DeclaringType != null && FindOn(called.DeclaringType, attributeType) is DecorateAttribute onCalledType)
            return onCalledType;

        return null;
    }

    private static DecorateAttribute FindOn(MemberInfo member, Type attributeType)
        => (DecorateAttribute)Attribute.GetCustomAttributes(member, attributeType, inherit: true).FirstOrDefault();
}
