﻿using Microsoft.AspNetCore.Builder;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Middleware;

namespace Milvasoft.MultiTenancy.Extensions;

/// <summary>
/// Provides registration of custom tenant middlewares.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Use the Tenant Middleware to process the request
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy<TTenant, TKey>(this IApplicationBuilder builder)
        where TTenant : class, IMilvaTenantBase<TKey>
        where TKey : struct, IEquatable<TKey>
        => builder.UseMiddleware<TenantMiddleware<TTenant, TKey>>();

    /// <summary>
    /// Use the Tenant Auth to process the authentication handlers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenantAuthentication(this IApplicationBuilder builder)
        => builder.UseMiddleware<TenantAuthMiddleware>();
}
