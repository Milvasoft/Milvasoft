﻿using Microsoft.AspNetCore.Http;

namespace Milvasoft.MultiTenancy.ResolutionStrategy;

/// <summary>
/// Resolve the host to a tenant identifier
/// </summary>
/// <remarks>
/// Creates new instance of <see cref="HostResolutionStrategy"/>
/// </remarks>
/// <param name="httpContextAccessor"></param>
public class HostResolutionStrategy(IHttpContextAccessor httpContextAccessor) : ITenantResolutionStrategy<string>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Get the tenant identifier from host.
    /// </summary>
    /// <returns></returns>
    public Task<string> GetTenantIdentifierAsync() => Task.FromResult(_httpContextAccessor.HttpContext.Request.Host.Host);

    /// <summary>
    /// Get the tenant identifier from host.
    /// </summary>
    /// <returns></returns>
    public string GetTenantIdentifier() => _httpContextAccessor.HttpContext.Request.Host.Host;
}
