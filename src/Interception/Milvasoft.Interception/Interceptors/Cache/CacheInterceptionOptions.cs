using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Builder;

namespace Milvasoft.Interception.Interceptors.Cache;

/// <summary>
/// Represents the options for cache interception.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "This value will be assigned only at application startup and acts as a singleton. Therefore, there is no harm in this use.")]
public class CacheInterceptionOptions : ICacheInterceptionOptions
{
    private static Type _accessorType = null;
    private string _cacheAccessorAssemblyQualifiedName;

    /// <inheritdoc/>
    public static string SectionName { get; } = $"{MilvaConstant.ParentSectionName}:Interception:Cache";

    /// <inheritdoc/>
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <inheritdoc/>
    public bool IncludeRequestHeadersWhenCaching { get; set; } = true;

    /// <inheritdoc/>
    public string CacheAccessorAssemblyQualifiedName
    {
        get => _cacheAccessorAssemblyQualifiedName;
        set
        {
            _cacheAccessorAssemblyQualifiedName = value;
            _accessorType = Type.GetType(_cacheAccessorAssemblyQualifiedName);
        }
    }

    /// <inheritdoc/>
    public Type CacheAccessorType
    {
        get => _accessorType;
        set
        {
            _accessorType = typeof(ICacheAccessor<>).MakeGenericType(value);
            _cacheAccessorAssemblyQualifiedName = value.AssemblyQualifiedName;
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
    /// When a return value is to be cached, a cache key is created with the method name and the values of the method request parameters. 
    /// In cases where HttpRequestHeaders may cause differences in the requests made, the value returned by this delegate is appended to the cache key.
    /// If this value is true request headers will included to creation of cache key.
    /// </summary>
    public bool IncludeRequestHeadersWhenCaching { get; set; }

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
