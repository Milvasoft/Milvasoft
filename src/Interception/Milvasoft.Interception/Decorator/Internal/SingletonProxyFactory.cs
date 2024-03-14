using Castle.DynamicProxy;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// Castle.Dynamic proxy singleton generator.
/// </summary>
internal sealed class SingletonProxyFactory
{
    private static ProxyGenerator _proxyGenerator;

    private SingletonProxyFactory()
    {
    }

    public static ProxyGenerator GetProxyGenerator() => _proxyGenerator ??= new ProxyGenerator();
}
