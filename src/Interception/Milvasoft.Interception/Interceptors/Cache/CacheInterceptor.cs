using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

/// <summary>
/// It ensures that the return value of the method marked with the <see cref="CacheAttribute"/> is cached with <see cref="ICacheAccessor"/> after the first call,
/// and that the method returns from the cache during the specified timeout period when called with the same parameters.
/// </summary>
public class CacheInterceptor : IMilvaInterceptor
{
    private readonly ICacheAccessor _cache;
    private readonly ICacheInterceptionOptions _interceptionOptions;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = 0;

    /// <summary>
    /// Initializes new instance with <paramref name="serviceProvider"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _interceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        if (_interceptionOptions != null && _interceptionOptions.CacheAccessorType != null)
        {
            var accessorType = _interceptionOptions.GetAccessorType();

            _cache = (ICacheAccessor)serviceProvider.GetService(accessorType);
        }
    }

    /// <inheritdoc/>
    public async Task OnInvoke(Call call)
    {
        //If cache provider is null do nothing, just proceed to next invocation
        if (_cache == null)
        {
            await call.NextAsync();
            return;
        }

        string cacheKey = BuildCacheKey(call, _serviceProvider, _interceptionOptions);

        var returnType = call.ReturnType.IsGenericType && call.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                                ? call.ReturnType.GenericTypeArguments.FirstOrDefault()
                                : call.ReturnType;

        var cachedValue = await _cache.GetAsync(cacheKey, returnType);

        //If cached value is not null make assignments and proceed next
        if (cachedValue != null)
        {
            if (returnType.CanAssignableTo(typeof(IResponse)))
                cachedValue.GetType().GetProperty("IsCachedData").SetValue(cachedValue, true);

            call.ReturnValue = cachedValue;
            call.ProceedToOriginalInvocation = false;

            await call.NextAsync();
            return;
        }

        await call.NextAsync();

        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        //If cached value is not null this block will not run
        TimeSpan? expirationSecond = cacheAttribute.Timeout.HasValue ? TimeSpan.FromSeconds(cacheAttribute.Timeout.Value) : null;

        await _cache.SetAsync(cacheKey, call.ReturnValue, expirationSecond);

        cacheAttribute.IsProcessedOnce = true;
    }

    private string BuildCacheKey(Call call, IServiceProvider serviceProvider, ICacheInterceptionOptions interceptionOptions)
    {
        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        string cacheKey = string.IsNullOrWhiteSpace(cacheAttribute.Key) ? call.Method.Name : cacheAttribute.Key;

        var methodParameters = call.Arguments?.ToList();

        methodParameters?.RemoveAll(p => p is CancellationToken);

        var requestHeaders = interceptionOptions.IncludeRequestHeadersWhenCaching ? RequestHeadersForCacheKey(serviceProvider) : string.Empty;

        var customKeyArguments = interceptionOptions.CacheKeyConfigurator != null ? interceptionOptions.CacheKeyConfigurator.Invoke(serviceProvider) : string.Empty;

        cacheKey = $"{cacheKey}_{methodParameters?.ToJson()}_{requestHeaders}_{customKeyArguments}".Hash();

        return cacheKey;
    }

    private string RequestHeadersForCacheKey(IServiceProvider serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

        if (httpContextAccessor?.HttpContext?.Request?.Headers != null)
        {
            var stringBuilder = new StringBuilder();

            foreach (var header in httpContextAccessor.HttpContext.Request.Headers)
            {
                if (_interceptionOptions.IgnoredRequestHeaderKeys.Contains(header.Key))
                    continue;

                stringBuilder.Append(header.Value);
            }

            return stringBuilder.ToString();
        }

        return string.Empty;
    }
}