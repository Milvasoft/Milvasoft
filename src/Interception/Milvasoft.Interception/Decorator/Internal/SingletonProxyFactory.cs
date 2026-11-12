using Castle.DynamicProxy;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Castle.Dynamic proxy singleton generator.
/// </summary>
internal static class SingletonProxyFactory
{
    // A single generator (and its dynamic assembly / proxy type cache) for the whole process.
    private static readonly ProxyGenerator _proxyGenerator = new();

    public static ProxyGenerator GetProxyGenerator() => _proxyGenerator;
}
