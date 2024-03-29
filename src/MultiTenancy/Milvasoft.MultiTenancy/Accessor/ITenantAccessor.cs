﻿using Microsoft.AspNetCore.Http;
using Milvasoft.Core.EntityBases.MultiTenancy;

namespace Milvasoft.MultiTenancy.Accessor;

/// <summary>
/// Tenant accessor for easy access.
/// </summary>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ITenantAccessor<TTenant, TKey>
    where TTenant : class, IMilvaTenantBase<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Application service provider.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Context accessor.
    /// </summary>
    IHttpContextAccessor HttpContextAccessor { get; }

    /// <summary>
    /// Accessed tenant from <see cref="HttpContext"/>
    /// </summary>
    TTenant Tenant { get; }
}
