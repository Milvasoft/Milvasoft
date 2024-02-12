using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions.Cache;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptor : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = 0;

    private IServiceProvider _serviceProvider;
    private ICacheAccessor _cache;
    private ICacheInterceptionOptions _cacheInterceptionOptions;

    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cacheInterceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        if (_cacheInterceptionOptions != null && _cacheInterceptionOptions.CacheAccessorType != null)
            _cache = (ICacheAccessor)serviceProvider.GetService(_cacheInterceptionOptions.CacheAccessorType);
    }

    public async Task OnInvoke(Call call)
    {
        if (_cache != null)
        {
            object cachedValue = default;
            string cacheKey = null;

            var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

            if (cacheAttribute.Key != null)
            {
                cacheKey = $"{cacheAttribute.Key}_{call.Arguments.ToJson()}";

                var value = await _cache.GetAsync<object>(cacheKey);

                if (value != null)
                {
                    if (value is IResponse)
                    {
                        value.GetType().GetProperty("IsCachedData").SetValue(value, true);
                    }

                    cachedValue = value;
                    call.ReturnValue = cachedValue;
                    call.ProceedToOriginalInvocation = false;
                    await call.NextAsync();
                    return;
                }
            }

            await call.NextAsync();

            if (cachedValue == null)
            {
                if (cacheAttribute.Timeout.HasValue)
                {
                    TimeSpan timespan = TimeSpan.FromSeconds(cacheAttribute.Timeout.Value);
                    await _cache.SetAsync(cacheKey, call.ReturnValue, timespan);
                }
                else
                {
                    await _cache.SetAsync(cacheKey, call.ReturnValue);
                }

                cacheAttribute.IsProcessedOnce = true;
            }
            else
            {
                if (call.ReturnValue?.GetType() == typeof(IResponse<>))
                {
                    call.ReturnValue.GetType().GetProperty("IsCachedData").SetValue(call.ReturnValue, true);
                }

            }
        }
        else await call.NextAsync();
    }

}