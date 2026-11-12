using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

internal static class TypeExtensions
{
    /// <summary>
    /// Finds public methods that do not have special names.
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
        // ReflectedType is the concrete type the method was fetched from; it may implement interfaces through inherited methods.
        var declaringType = implementation.ReflectedType ?? implementation.DeclaringType;

        if (declaringType == null || declaringType.IsInterface)
            return null;

        var method = implementation.IsGenericMethod && !implementation.IsGenericMethodDefinition
                        ? implementation.GetGenericMethodDefinition()
                        : implementation;

        foreach (var @interface in declaringType.GetInterfaces())
        {
            var map = declaringType.GetInterfaceMap(@interface);

            // Compare handles instead of MethodInfo instances, their ReflectedType may differ for inherited methods.
            var index = Array.FindIndex(map.TargetMethods, target => target.MethodHandle == method.MethodHandle);

            if (index != -1)
                return map.InterfaceMethods[index];
        }

        return null;
    }
}
