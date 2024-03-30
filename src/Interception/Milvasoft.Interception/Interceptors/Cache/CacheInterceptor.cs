using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptor : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = 0;

    private readonly ICacheAccessor _cache;

    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        var cacheInterceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        if (cacheInterceptionOptions != null && cacheInterceptionOptions.CacheAccessorAssemblyQualifiedName != null)
        {
            var accessorType = cacheInterceptionOptions.GetAccessorType();

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

        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        string cacheKey = string.IsNullOrWhiteSpace(cacheAttribute.Key) ? call.Method.Name : cacheAttribute.Key;

        var methodParameters = call.Arguments?.ToList().RemoveAll(p => p is CancellationToken);

        cacheKey = $"{cacheKey}_{methodParameters?.ToJson().Hash()}";

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

        //If cached value is not null this block will not run
        TimeSpan? expirationSecond = cacheAttribute.Timeout.HasValue ? TimeSpan.FromSeconds(cacheAttribute.Timeout.Value) : null;

        await _cache.SetAsync(cacheKey, call.ReturnValue, expirationSecond);

        cacheAttribute.IsProcessedOnce = true;
    }
}