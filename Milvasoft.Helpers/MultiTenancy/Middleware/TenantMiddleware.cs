using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Service;
using Milvasoft.Helpers.MultiTenancy.Utils;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.MultiTenancy.Middleware;

/// <summary>
/// If request items not contains Tenant object. Sets the tenant object into items.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class TenantMiddleware<TTenant, TKey>
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes new instance of <see cref="TenantMiddleware{TTenant, TKey}"/>
    /// </summary>
    /// <param name="next"></param>
    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the method or constructor reflected by this MethodInfo instance.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        if (!context.Items.ContainsKey(TenancyConstants.HttpContextTenantKey))
        {
            var tenantService = context.RequestServices.GetService(typeof(ITenantService<TTenant, TKey>)) as ITenantService<TTenant, TKey>;

            var tenant = await tenantService.GetTenantAsync().ConfigureAwait(false);

            if (tenant != null)
                context.Items.Add(TenancyConstants.HttpContextTenantKey, tenant);
        }

        //Continue processing
        if (_next != null)
            await _next.Invoke(context);
    }
}
