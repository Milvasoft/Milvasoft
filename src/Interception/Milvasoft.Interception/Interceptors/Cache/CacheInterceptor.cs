﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.Cache;

public class CacheInterceptor : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = 0;

    private readonly ICacheAccessor _cache;
    private readonly ICacheInterceptionOptions _cacheInterceptionOptions;

    public CacheInterceptor(IServiceProvider serviceProvider)
    {
        _cacheInterceptionOptions = serviceProvider.GetService<ICacheInterceptionOptions>();

        if (_cacheInterceptionOptions != null && _cacheInterceptionOptions.CacheAccessorAssemblyQualifiedName != null)
        {
            var accessorType = _cacheInterceptionOptions.GetAccessorType();

            _cache = (ICacheAccessor)serviceProvider.GetService(accessorType);
        }
    }

    public async Task OnInvoke(Call call)
    {
        if (_cache != null)
        {
            object cachedValue = default;
            string cacheKey = null;

            var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

            if (cacheAttribute?.Key != null)
            {
                var methodParameters = call.Arguments?.ToList();

                methodParameters?.RemoveAll(p => p is CancellationToken);

                cacheKey = $"{cacheAttribute.Key}_{methodParameters?.ToJson().Hash()}";

                var returnType = call.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ? call.ReturnType.GenericTypeArguments.FirstOrDefault() : call.ReturnType;

                var value = await _cache.GetAsync(cacheKey, returnType);

                if (value != null)
                {
                    if (returnType.IsAssignableTo(typeof(IResponse)))
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
        else
            await call.NextAsync();
    }
}