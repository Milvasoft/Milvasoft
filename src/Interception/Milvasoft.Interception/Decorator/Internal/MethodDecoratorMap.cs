using Milvasoft.Components.Rest.MilvaResponse;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Which decorators intercept a method, as declared by attributes.
/// </summary>
/// <param name="DecoratorTypes">Interceptor types declared with <see cref="DecorateAttribute"/>s on the method, its interface method or the class.</param>
/// <param name="ReturnsResponse">Whether the method returns an <see cref="IResponse"/> (directly or as the result of a task).</param>
internal sealed record MethodDecoration(Type[] DecoratorTypes, bool ReturnsResponse);

/// <summary>
/// Mappings of which decorator will intercept decorable methods.
/// The map is built once per decorated type from attributes only; anything that depends on the service provider is decided when a proxy is created.
/// </summary>
internal static class MethodDecoratorMap
{
    private static readonly ConcurrentDictionary<Type, ReadOnlyDictionary<MethodInfo, MethodDecoration>> _map = new();

    /// <summary>
    /// Gets (and caches) the decorations of the given type's methods. Methods without any decoration are not included.
    /// </summary>
    /// <param name="decoratedType"></param>
    /// <returns></returns>
    public static ReadOnlyDictionary<MethodInfo, MethodDecoration> Get(Type decoratedType) => _map.GetOrAdd(decoratedType, Factory);

    /// <summary>
    /// Finds the decorable methods of the class of the given type. Then, it maps which decorators will intercept these methods.
    /// </summary>
    /// <param name="decoratedType"></param>
    /// <returns></returns>
    private static ReadOnlyDictionary<MethodInfo, MethodDecoration> Factory(Type decoratedType)
    {
        var map = new Dictionary<MethodInfo, MethodDecoration>();

        var classAttributes = decoratedType.GetCustomAttributes<DecorateAttribute>(true).ToList();

        foreach (var method in decoratedType.GetDecoratableMethods())
        {
            var decoratorAttributes = method.GetCustomAttributes<DecorateAttribute>(true).Concat(classAttributes);

            var interfaceMethod = method.GetInterfaceMethod();

            if (interfaceMethod != null)
                decoratorAttributes = decoratorAttributes.Concat(interfaceMethod.GetCustomAttributes<DecorateAttribute>(true));

            var decoratorTypes = decoratorAttributes.Select(attribute => attribute.DecoratorType).Distinct().ToArray();

            var returnsResponse = ReturnsResponse(method.ReturnType);

            if (decoratorTypes.Length == 0 && !returnsResponse)
                continue;

            map[method] = new MethodDecoration(decoratorTypes, returnsResponse);
        }

        return new ReadOnlyDictionary<MethodInfo, MethodDecoration>(map);
    }

    private static bool ReturnsResponse(Type returnType)
    {
        if (returnType.CanAssignableTo(typeof(IResponse)))
            return true;

        if (returnType.IsGenericType)
        {
            var definition = returnType.GetGenericTypeDefinition();

            if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
                return returnType.GetGenericArguments()[0].CanAssignableTo(typeof(IResponse));
        }

        return false;
    }
}
