using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptor : IMilvaInterceptor
{
    private readonly ICacheAccessor _cache;
    private readonly ICacheInterceptionOptions _interceptionOptions;
    private readonly IServiceProvider _serviceProvider;

    public int InterceptionOrder { get; set; } = 0;

    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _interceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        if (_interceptionOptions != null && _interceptionOptions.CacheAccessorAssemblyQualifiedName != null)
        {
            var accessorType = _interceptionOptions.GetAccessorType();

            _cache = (ICacheAccessor)serviceProvider.GetService(accessorType);
        }
    }

    public async Task OnInvoke(Call call)
    {
        //If cache provider is null nothing do nothing, just proceed to next
        if (_cache == null)
        {
            await call.NextAsync();
            return;
        }

        string cacheKey = BuildCacheKey(call, _serviceProvider, _interceptionOptions);

        var returnType = call.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ? call.ReturnType.GenericTypeArguments.FirstOrDefault() : call.ReturnType;

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

    private static string BuildCacheKey(Call call, IServiceProvider serviceProvider, ICacheInterceptionOptions interceptionOptions)
    {
        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        string cacheKey = string.IsNullOrWhiteSpace(cacheAttribute.Key) ? call.Method.Name : cacheAttribute.Key;

        var methodParameters = call.Arguments?.ToList().RemoveAll(p => p is CancellationToken);

        var requestHeaders = interceptionOptions.IncludeRequestHeadersWhenCaching ? RequestHeadersForCacheKey(serviceProvider) : string.Empty;

        var customKeyArguments = interceptionOptions.CacheKeyConfigurator != null ? interceptionOptions.CacheKeyConfigurator.Invoke(serviceProvider) : string.Empty;

        cacheKey = $"{cacheKey}_{methodParameters?.ToJson()}_{requestHeaders}_{customKeyArguments}".Hash();

        return cacheKey;
    }

    private static string RequestHeadersForCacheKey(IServiceProvider serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

        if (httpContextAccessor != null)
        {
            var httpContext = httpContextAccessor.HttpContext;

            var stringBuilder = new StringBuilder();

            foreach (var header in httpContext.Request.Headers)
            {
                stringBuilder.Append(header.Value);
            }

            return stringBuilder.ToString();
        }

        return string.Empty;
    }
}