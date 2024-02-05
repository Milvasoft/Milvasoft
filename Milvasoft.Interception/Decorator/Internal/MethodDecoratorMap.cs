using Milvasoft.Components.Rest.Response;
using Milvasoft.Interception.Interceptors.Logging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Mappings of which decorator will intercept decorable methods.
/// </summary>
internal class MethodDecoratorMap
{
    private readonly ConcurrentDictionary<Type, ReadOnlyDictionary<MethodInfo, Type[]>> _map;

    public MethodDecoratorMap() => _map = new ConcurrentDictionary<Type, ReadOnlyDictionary<MethodInfo, Type[]>>();

    public ReadOnlyDictionary<MethodInfo, Type[]> Get(Type decoratedType) => _map.GetOrAdd(decoratedType, Factory);

    /// <summary>
    /// Finds the decorable methods of the class of the given type. Then, it maps which decorators will intercept these methods.
    /// </summary>
    /// <param name="decoratedType"></param>
    /// <returns></returns>
    private static ReadOnlyDictionary<MethodInfo, Type[]> Factory(Type decoratedType)
    {
        var map = new Dictionary<MethodInfo, Type[]>();

        var methods = decoratedType.GetDecoratableMethods();

        var classAttributes = decoratedType.GetCustomAttributes<DecorateAttribute>(true);

        foreach (var method in methods)
        {
            var decoratorAttributes = method.GetCustomAttributes<DecorateAttribute>(true);

            if (classAttributes != null)
                decoratorAttributes = decoratorAttributes.Concat(classAttributes);

            var interfaceMethod = method.GetInterfaceMethod();

            if (interfaceMethod != null)
                decoratorAttributes = decoratorAttributes.Concat(interfaceMethod.GetCustomAttributes<DecorateAttribute>(true));

            var decorators = decoratorAttributes.Select(attribute => attribute.DecoratorType).Distinct().ToList();


            var isMilvaResponseTyped = method.ReturnType.IsAssignableTo(typeof(IResponse));

            if (method.ReturnType.GetTypeInfo().IsGenericType && method.ReturnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Task<>))
            {
                var typeOfTask = method.ReturnType.GetGenericArguments()[0];
                isMilvaResponseTyped = typeOfTask.IsAssignableTo(typeof(IResponse));
            }

            if (isMilvaResponseTyped)
                decorators.Add(typeof(ResponseInterceptor));

            map.Add(method, decorators.ToArray());
        }

        return new ReadOnlyDictionary<MethodInfo, Type[]>(map);
    }
}
