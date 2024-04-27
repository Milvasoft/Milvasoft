using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Interceptors.Response;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Mappings of which decorator will intercept decorable methods.
/// </summary>
internal static class MethodDecoratorMap
{
    private readonly static ConcurrentDictionary<Type, ReadOnlyDictionary<MethodInfo, Type[]>> _map = [];

    /// <summary>
    /// Gets or adds decorated type with factory.
    /// </summary>
    /// <param name="decoratedType"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static ReadOnlyDictionary<MethodInfo, Type[]> Get(Type decoratedType, IServiceProvider serviceProvider) => _map.GetOrAdd(decoratedType, Factory(decoratedType, serviceProvider));

    /// <summary>
    /// Finds the decorable methods of the class of the given type. Then, it maps which decorators will intercept these methods.
    /// </summary>
    /// <param name="decoratedType"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    private static ReadOnlyDictionary<MethodInfo, Type[]> Factory(Type decoratedType, IServiceProvider serviceProvider)
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

            var isMilvaResponseTyped = method.ReturnType.CanAssignableTo(typeof(IResponse));

            if (!isMilvaResponseTyped && method.ReturnType.GetTypeInfo().IsGenericType && method.ReturnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Task<>))
            {
                var typeOfTask = method.ReturnType.GetGenericArguments()[0];
                isMilvaResponseTyped = typeOfTask.IsAssignableTo(typeof(IResponse));
            }

            if (isMilvaResponseTyped && serviceProvider.GetService<ResponseInterceptor>() != null)
                decorators.Add(typeof(ResponseInterceptor));

            map.Add(method, [.. decorators]);
        }

        return new ReadOnlyDictionary<MethodInfo, Type[]>(map);
    }
}
