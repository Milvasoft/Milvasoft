using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Builder;

namespace Milvasoft.Interception.Interceptors.Cache;

/// <summary>
/// Represents the options for cache interception.
/// </summary>
public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    private static Type _accessorType = null;
    private string _cacheAccessorAssemblyQualifiedName;

    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Cache";

    /// <inheritdoc/>
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <inheritdoc/>
    public HashSet<string> IncludedRequestHeaderKeys { get; set; } = [];

    /// <inheritdoc/>
    public string CacheAccessorAssemblyQualifiedName
    {
        get => _cacheAccessorAssemblyQualifiedName;
        set
        {
            if (value is not null)
            {
                _cacheAccessorAssemblyQualifiedName = value;
                _accessorType = Type.GetType(_cacheAccessorAssemblyQualifiedName);
            }
        }
    }

    /// <inheritdoc/>
    public Type CacheAccessorType
    {
        get => _accessorType;
        set
        {
            if (value is not null)
            {
                _accessorType = typeof(ICacheAccessor<>).MakeGenericType(value);
                _cacheAccessorAssemblyQualifiedName = value.AssemblyQualifiedName;
            }
        }
    }

    /// <inheritdoc/>
    public Func<IServiceProvider, string> CacheKeyConfigurator { get; set; }

    /// <inheritdoc/>
    public Type GetAccessorType() => _accessorType ??= typeof(ICacheAccessor<>).MakeGenericType(Type.GetType(_cacheAccessorAssemblyQualifiedName));
}

/// <summary>
/// Represents the options for cache interception.
/// </summary>
public interface ICacheInterceptionOptions : IInterceptionOptions
{
    /// <summary>
    /// Keys ​​of the headers to be included in the cache key creation process. If this property is null or empty; request headers will not included.
    ///
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters.
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// </summary>
    public HashSet<string> IncludedRequestHeaderKeys { get; set; }

    /// <summary>
    /// Cache accessor assembly qualified name for configuring options from configuration file.
    /// For example 'Milvasoft.Caching.Redis.RedisAccessor, Milvasoft.Caching.Redis, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null'
    /// </summary>
    public string CacheAccessorAssemblyQualifiedName { get; set; }

    /// <summary>
    /// Cache accessor type. For example typeof(RedisAccessor)
    /// </summary>
    public Type CacheAccessorType { get; set; }

    /// <summary>
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters.
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// </summary>
    public Func<IServiceProvider, string> CacheKeyConfigurator { get; set; }

    /// <summary>
    /// Gets the generic accessor type as <see cref="ICacheAccessor{TAccessor}"/>.
    /// </summary>
    /// <returns>The generic accessor type.</returns>
    public Type GetAccessorType();
}
