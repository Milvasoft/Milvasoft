using Microsoft.AspNetCore.Http;
using Milvasoft.Helpers.MultiTenancy.EntityBase;
using Milvasoft.Helpers.MultiTenancy.Utils;
using System;

namespace Milvasoft.Helpers.MultiTenancy.Extensions;

/// <summary>
/// Extensions to HttpContext to make multi-tenancy easier to use
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the current tenant
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TTenant GetTenant<TTenant, TKey>(this HttpContext context)
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        if (!context.Items.ContainsKey(TenancyConstants.HttpContextTenantKey))
            return null;
        return context.Items[TenancyConstants.HttpContextTenantKey] as TTenant;
    }
}
