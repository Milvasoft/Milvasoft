using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Attributes;
using Milvasoft.MultiTenancy.Context;
using Milvasoft.MultiTenancy.Options;
using Milvasoft.MultiTenancy.ResolutionStrategy;
using Milvasoft.MultiTenancy.Store;
using Milvasoft.MultiTenancy.Utils;
using System.Security.Claims;

namespace Milvasoft.MultiTenancy.Middleware;

/// <summary>
/// Resolves and validates the tenant of the request once and stores it in the scoped <see cref="ITenantContext{TTenant, TKey}"/>
/// (and in <see cref="HttpContext.Items"/> for <see cref="Extensions.HttpContextExtensions.GetTenant{TTenant, TKey}(HttpContext)"/>).
/// </summary>
/// <remarks>
/// <para>Requests without a valid identifier, with an unknown tenant, or (when <see cref="TenantResolutionOptions.TenantClaimType"/> is set)
/// whose authenticated user belongs to another tenant are rejected, unless the tenant is optional for the request
/// (<see cref="AllowWithoutTenantAttribute"/> or <see cref="TenantResolutionOptions.IsTenantOptional"/>). A claim mismatch is always rejected.</para>
/// <para>When the claim check is enabled and authentication has not run yet, the default authentication scheme is evaluated here.</para>
/// </remarks>
/// <typeparam name="TTenant"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <param name="next"></param>
public class TenantMiddleware<TTenant, TKey>(RequestDelegate next)
where TTenant : class, IMilvaTenantBase<TKey>
where TKey : struct, IEquatable<TKey>
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Invokes the method or constructor reflected by this MethodInfo instance.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        var services = context.RequestServices;
        var options = services.GetService<TenantResolutionOptions>() ?? new TenantResolutionOptions();
        var tenantContext = services.GetRequiredService<ITenantContext<TTenant, TKey>>();

        if (!tenantContext.IsResolved)
        {
            var rejection = await ResolveAsync(context, tenantContext, options);

            if (rejection is TenantRejectionReason reason)
            {
                await RejectAsync(context, options, reason);
                return;
            }
        }

        //Continue processing
        if (_next != null)
            await _next.Invoke(context);
    }

    private static async Task<TenantRejectionReason?> ResolveAsync(HttpContext context, ITenantContext<TTenant, TKey> tenantContext, TenantResolutionOptions options)
    {
        var services = context.RequestServices;
        var isTenantOptional = IsTenantOptional(context, options);

        var strategy = services.GetRequiredService<ITenantResolutionStrategy<TKey>>();

        var identifier = await strategy.GetTenantIdentifierAsync();

        if (TenantIdentifierHelper.IsEmpty(identifier))
            return isTenantOptional ? null : TenantRejectionReason.Missing;

        var store = services.GetRequiredService<ITenantStore<TTenant, TKey>>();

        var tenant = await store.GetTenantAsync(identifier);

        if (tenant == null || !TenantIdentifierHelper.AreEqual(tenant.Id, identifier))
            return isTenantOptional ? null : TenantRejectionReason.NotFound;

        if (!string.IsNullOrWhiteSpace(options.TenantClaimType) && !await UserBelongsToTenantAsync(context, options.TenantClaimType, identifier))
            return TenantRejectionReason.ClaimMismatch;

        tenantContext.SetTenant(tenant);
        context.Items[TenancyConstants.HttpContextTenantKey] = tenant;

        return null;
    }

    private static bool IsTenantOptional(HttpContext context, TenantResolutionOptions options)
        => context.GetEndpoint()?.Metadata.GetMetadata<AllowWithoutTenantAttribute>() != null
           || (options.IsTenantOptional?.Invoke(context) ?? false);

    /// <summary>
    /// Anonymous users are allowed; authenticated users must carry the claim with the requested tenant.
    /// </summary>
    private static async Task<bool> UserBelongsToTenantAsync(HttpContext context, string claimType, TKey identifier)
    {
        var principal = await GetAuthenticatedPrincipalAsync(context);

        if (principal == null)
            return true;

        var claimValue = principal.FindFirst(claimType)?.Value;

        return claimValue != null && string.Equals(claimValue, identifier.ToString(), StringComparison.Ordinal);
    }

    private static async Task<ClaimsPrincipal> GetAuthenticatedPrincipalAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
            return context.User;

        // Authentication middleware may run after this middleware; evaluate the default scheme (handlers cache the result per request).
        var schemeProvider = context.RequestServices.GetService<IAuthenticationSchemeProvider>();

        if (schemeProvider == null || await schemeProvider.GetDefaultAuthenticateSchemeAsync() == null)
            return null;

        var result = await context.AuthenticateAsync();

        return result?.Succeeded == true && result.Principal?.Identity?.IsAuthenticated == true ? result.Principal : null;
    }

    private static Task RejectAsync(HttpContext context, TenantResolutionOptions options, TenantRejectionReason reason)
    {
        if (options.OnRejected != null)
            return options.OnRejected(context, reason);

        context.Response.StatusCode = reason switch
        {
            TenantRejectionReason.NotFound => StatusCodes.Status404NotFound,
            TenantRejectionReason.ClaimMismatch => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        return Task.CompletedTask;
    }
}
