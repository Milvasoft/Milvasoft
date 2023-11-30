using System.Reflection;
using System.Runtime.CompilerServices;

namespace Milvasoft.Interception.Decorator.Internal;

internal static class TypeExtensions
{
    private static readonly Type _voidTaskResultType = Type.GetType("System.Threading.Tasks.VoidTaskResult", false);

    /// <summary>
    /// Finds public methods that do not have <see cref="MethodInfo.IsSpecialName"/>.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static IEnumerable<MethodInfo> GetDecoratableMethods(this Type type)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
               .Where(x => !x.IsSpecialName);


    /// <summary>
    /// Gets <paramref name="implementation"/>'s interface methods as method info.
    /// </summary>
    /// <param name="implementation"></param>
    /// <returns></returns>
    internal static MethodInfo GetInterfaceMethod(this MethodInfo implementation)
    {
        var interfaces = implementation.DeclaringType.GetInterfaces();

        foreach (var @interface in interfaces)
        {
            var map = implementation.DeclaringType.GetInterfaceMap(@interface);

            var index = Array.IndexOf(map.TargetMethods, implementation);

            if (index != -1)
            {
                return map.InterfaceMethods[index];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns <paramref name="method"/> is async method or not.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    internal static bool IsAsync(this MethodInfo method)
        => method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;

    /// <summary>
    /// Returns <paramref name="type"/>'s return type is <see cref="System.Threading.Tasks.VoidTaskResult"/> or not.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static bool IsTaskWithVoidTaskResult(this Type type)
        => type.GenericTypeArguments?.Length > 0 && type.GenericTypeArguments[0] == _voidTaskResultType;

    /// <summary>
    /// Returns <paramref name="type"/>'s return type is <see cref="Task{}"/> or not.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericTaskType"></param>
    /// <returns></returns>
    internal static bool TryGetGenericTaskType(this TypeInfo type, out TypeInfo genericTaskType)
    {
        ArgumentNullException.ThrowIfNull(type);

        while (type != null)
        {
            if (IsGenericTaskType(type))
            {
                genericTaskType = type;
                return true;
            }

            type = type.BaseType.GetTypeInfo();
        }

        genericTaskType = null;
        return false;

        static bool IsGenericTaskType(TypeInfo typeInfo) => typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Task<>);
    }
}
