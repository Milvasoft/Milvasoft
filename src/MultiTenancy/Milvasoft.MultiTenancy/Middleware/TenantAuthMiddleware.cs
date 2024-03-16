using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.MultiTenancy.Middleware;

/// <summary>
/// AuthenticationMiddleware.cs from framework with injection point moved
/// </summary>
/// <remarks>
/// Initializes new instance of <see cref="TenantAuthMiddleware"/>
/// </remarks>
/// <param name="next"></param>
public class TenantAuthMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// Invokes the method or constructor reflected by this MethodInfo instance.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="schemes"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
    {
        context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
        {
            OriginalPath = context.Request.Path,
            OriginalPathBase = context.Request.PathBase
        });

        // Give any IAuthenticationRequestHandler schemes a chance to handle the request
        var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
        foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
        {
            if (await handlers.GetHandlerAsync(context, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                return;
        }

        var defaultAuthenticate = await schemes.GetDefaultAuthenticateSchemeAsync();
        if (defaultAuthenticate != null)
        {
            var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
            if (result?.Principal != null)
            {
                context.User = result.Principal;
            }
        }

        await _next.Invoke(context);
    }
}
