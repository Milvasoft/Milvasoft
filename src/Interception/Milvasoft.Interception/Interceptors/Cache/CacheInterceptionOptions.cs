using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    private static Type _accessorType = null;

    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Cache";
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;
    public string CacheAccessorAssemblyQualifiedName { get; set; }
    public bool IncludeRequestHeadersWhenCaching { get; set; } = true;

    /// <summary>
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters. 
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// </summary>
    public Func<IServiceProvider, string> CacheKeyConfigurator { get; set; }

    /// <summary>
    /// Gets generic accessor type as <see cref="ICacheAccessor{TAccessor}"/>
    /// </summary>
    /// <returns></returns>
#pragma warning disable S2696 // Instance members should not write to "static" fields
    public Type GetAccessorType() => _accessorType ??= typeof(ICacheAccessor<>).MakeGenericType(Type.GetType(CacheAccessorAssemblyQualifiedName));
#pragma warning restore S2696 // Instance members should not write to "static" fields
}

public interface ICacheInterceptionOptions : IMilvaOptions
{
    public ServiceLifetime InterceptorLifetime { get; set; }
    public string CacheAccessorAssemblyQualifiedName { get; set; }
    public bool IncludeRequestHeadersWhenCaching { get; set; }

    /// <summary>
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters. 
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// </summary>
    public Func<IServiceProvider, string> CacheKeyConfigurator { get; set; }

    public Type GetAccessorType();
}