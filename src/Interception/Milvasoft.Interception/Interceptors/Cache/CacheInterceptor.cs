using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

/// <summary>
/// It ensures that the return value of the method marked with the <see cref="CacheAttribute"/> is cached with <see cref="ICacheAccessor"/> after the first call,
/// and that the method returns from the cache during the specified timeout period when called with the same parameters.
/// </summary>
/// <remarks>
/// <para>
/// The cache key is built from <see cref="CacheAttribute.Key"/> (or the declaring type's full name and the method name when no key is given),
/// the method arguments, the configured request headers and <see cref="ICacheInterceptionOptions.CacheKeyConfigurator"/>.
/// </para>
/// <para>Null results and unsuccessful <see cref="IResponse"/> results are not cached. Cache failures never fail the call; the method runs without cache instead.</para>
/// <para>
/// The cache interceptor runs inside the <see cref="Response.ResponseInterceptor"/> (it has a higher <see cref="InterceptionOrder"/>),
/// so the raw method result is cached and per request response processing (metadata, hiding, masking, translation) is applied to cached results too.
/// </para>
/// </remarks>
public class CacheInterceptor : IMilvaInterceptor
{
    private readonly ICacheAccessor _cache;
    private readonly ICacheInterceptionOptions _interceptionOptions;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = 1;

    /// <summary>
    /// Initializes new instance with <paramref name="serviceProvider"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _interceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        var accessorType = _interceptionOptions?.CacheAccessorType;

        if (accessorType != null)
        {
            _cache = serviceProvider.GetService(_interceptionOptions.GetAccessorType()) as ICacheAccessor;

            // The configured type itself may be the registered service (for example IRedisAccessor).
            if (_cache == null && typeof(ICacheAccessor).IsAssignableFrom(accessorType))
                _cache = serviceProvider.GetService(accessorType) as ICacheAccessor;
        }
    }

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        var returnType = call.ResultType;

        //If cache provider is null or there is nothing to cache, just proceed to next invocation
        if (_cache == null || returnType == typeof(void))
        {
            await call.NextAsync();
            return;
        }

        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        string cacheKey = BuildCacheKey(call, cacheAttribute);

        var cachedValue = await TryGetCachedValueAsync(cacheKey, returnType);

        //If cached value is not null make assignments and proceed next
        if (cachedValue != null)
        {
            if (cachedValue is IResponse cachedResponse)
                SetCachedDataFlag(cachedResponse);

            call.ReturnValue = cachedValue;
            call.ProceedToOriginalInvocation = false;

            await call.NextAsync();
            return;
        }

        await call.NextAsync();

        if (!ShouldCache(call.ReturnValue))
            return;

        TimeSpan? expiration = cacheAttribute?.Timeout is int timeout ? TimeSpan.FromSeconds(timeout) : null;

        await TrySetCachedValueAsync(cacheKey, call.ReturnValue, expiration);
    }

    private static bool ShouldCache(object returnValue) => returnValue switch
    {
        null => false,
        IResponse response => response.IsSuccess,
        _ => true
    };

    private static void SetCachedDataFlag(IResponse response)
    {
        var property = response.GetType().GetProperty("IsCachedData");

        if (property?.CanWrite == true && property.PropertyType == typeof(bool))
            property.SetValue(response, true);
    }

    private async Task<object> TryGetCachedValueAsync(string cacheKey, Type returnType)
    {
        try
        {
            return await _cache.GetAsync(cacheKey, returnType);
        }
        catch (Exception)
        {
            // A cache failure must not fail the call; the method runs without cache.
            return null;
        }
    }

    private async Task TrySetCachedValueAsync(string cacheKey, object value, TimeSpan? expiration)
    {
        try
        {
            await _cache.SetAsync(cacheKey, value, expiration);
        }
        catch (Exception)
        {
            // A cache failure must not change the result of a call that already completed.
        }
    }

    private string BuildCacheKey(Call call, CacheAttribute cacheAttribute)
    {
        var method = call.MethodImplementation ?? call.Method;

        string cacheKey = string.IsNullOrWhiteSpace(cacheAttribute?.Key)
                            ? $"{method.DeclaringType?.FullName}.{method.Name}{FormatGenericArguments(call.GenericArguments)}"
                            : cacheAttribute.Key;

        var methodParameters = call.Arguments?.Where(p => p is not CancellationToken);

        var requestHeaders = RequestHeadersForCacheKey();

        var customKeyArguments = _interceptionOptions.CacheKeyConfigurator != null ? _interceptionOptions.CacheKeyConfigurator.Invoke(_serviceProvider) : string.Empty;

        cacheKey = $"{cacheKey}_{methodParameters?.ToJson()}_{requestHeaders}_{customKeyArguments}".Hash();

        return cacheKey;
    }

    private static string FormatGenericArguments(Type[] genericArguments)
        => genericArguments.Length == 0 ? string.Empty : $"<{string.Join(",", genericArguments.Select(t => t.FullName))}>";

    private string RequestHeadersForCacheKey()
    {
        if (_interceptionOptions.IncludedRequestHeaderKeys.IsNullOrEmpty())
            return string.Empty;

        var headers = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.Request?.Headers;

        if (headers is null)
            return string.Empty;

        var stringBuilder = new StringBuilder();

        // Key and value are both part of the cache key and separated, so different header combinations cannot produce the same text.
        foreach (var key in _interceptionOptions.IncludedRequestHeaderKeys.Order(StringComparer.Ordinal))
        {
            if (headers.TryGetValue(key, out var value))
                stringBuilder.Append(key).Append('=').Append(value.ToString().Replace(";", ";;")).Append(';');
        }

        return stringBuilder.ToString();
    }
}
