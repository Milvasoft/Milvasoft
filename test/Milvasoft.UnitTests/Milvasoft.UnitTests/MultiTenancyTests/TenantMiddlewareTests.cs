using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.MultiTenancy.Attributes;
using Milvasoft.MultiTenancy.Context;
using Milvasoft.MultiTenancy.Extensions;
using Milvasoft.MultiTenancy.Middleware;
using Milvasoft.MultiTenancy.Options;
using Milvasoft.MultiTenancy.ResolutionStrategy;
using Milvasoft.MultiTenancy.Store;
using System.Security.Claims;

namespace Milvasoft.UnitTests.MultiTenancyTests;

[Trait("MultiTenancy Unit Tests", "Unit tests for Milvasoft.MultiTenancy tenant resolution.")]
public class TenantMiddlewareTests
{
    private const string _tenantClaimType = "tenant";

    [Fact]
    public async Task Request_WithKnownTenantHeader_ShouldResolveTenantOnceIntoTheScopedContext()
    {
        var (context, nextCalled) = await InvokeAsync(headerValue: "milva_1");

        nextCalled().Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        var tenantContext = context.RequestServices.GetRequiredService<ITenantContext<TestTenant, TenantId>>();
        tenantContext.IsResolved.Should().BeTrue();
        tenantContext.Tenant.Id.Should().Be(new TenantId("milva_1"));
        context.GetTenant<TestTenant, TenantId>().Should().BeSameAs(tenantContext.Tenant);
    }

    [Fact]
    public async Task Request_WithoutTenantHeader_ShouldBeRejected()
    {
        var (context, nextCalled) = await InvokeAsync(headerValue: null);

        nextCalled().Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.RequestServices.GetRequiredService<ITenantContext>().IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task Request_WithMalformedTenantHeader_ShouldBeRejectedWithoutThrowing()
    {
        var (context, nextCalled) = await InvokeAsync(headerValue: "not-a-tenant-id");

        nextCalled().Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Request_WithUnknownTenant_ShouldBeRejected()
    {
        var (context, nextCalled) = await InvokeAsync(headerValue: "unknown_1");

        nextCalled().Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Request_ToEndpointAllowingNoTenant_ShouldContinueWithoutTenant()
    {
        var (context, nextCalled) = await InvokeAsync(headerValue: null, endpointMetadata: new AllowWithoutTenantAttribute());

        nextCalled().Should().BeTrue();
        context.RequestServices.GetRequiredService<ITenantContext>().IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task Request_WhenTenantIsOptionalByOptions_ShouldContinueWithoutTenant()
    {
        var (_, nextCalled) = await InvokeAsync(headerValue: null, configure: opt => opt.IsTenantOptional = ctx => ctx.Request.Path.StartsWithSegments("/health"), path: "/health");

        nextCalled().Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticatedUserOfAnotherTenant_ShouldBeForbidden()
    {
        var user = CreateUser(new Claim(_tenantClaimType, "other_1"));

        var (context, nextCalled) = await InvokeAsync(headerValue: "milva_1", user: user, configure: opt => opt.TenantClaimType = _tenantClaimType);

        nextCalled().Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task AuthenticatedUserWithoutTenantClaim_ShouldBeForbiddenEvenOnOptionalEndpoints()
    {
        var user = CreateUser();

        var (context, nextCalled) = await InvokeAsync(headerValue: "milva_1", user: user, endpointMetadata: new AllowWithoutTenantAttribute(), configure: opt => opt.TenantClaimType = _tenantClaimType);

        nextCalled().Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task AuthenticatedUserOfTheRequestedTenant_ShouldBeAllowed()
    {
        var user = CreateUser(new Claim(_tenantClaimType, "milva_1"));

        var (_, nextCalled) = await InvokeAsync(headerValue: "milva_1", user: user, configure: opt => opt.TenantClaimType = _tenantClaimType);

        nextCalled().Should().BeTrue();
    }

    [Fact]
    public async Task CustomRejectionHandler_ShouldReceiveTheReason()
    {
        TenantRejectionReason? reason = null;

        var (context, _) = await InvokeAsync(headerValue: null, configure: opt => opt.OnRejected = (ctx, r) =>
        {
            reason = r;
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        });

        reason.Should().Be(TenantRejectionReason.Missing);
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void TenantContext_ShouldNotAllowSwitchingToAnotherTenant()
    {
        var tenantContext = new TenantContext<TestTenant, TenantId>();

        tenantContext.SetTenant(new TestTenant("milva", 1));

        var act = () => tenantContext.SetTenant(new TestTenant("other", 1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HeaderResolutionStrategy_WithMalformedHeader_ShouldReturnEmpty()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Request.Headers[HeaderResolutionStrategy.HeaderKey] = "malformed";

        new HeaderResolutionStrategy(accessor).GetTenantIdentifier().TenancyName.Should().BeNull();
    }

    #region Setup

    private static ClaimsPrincipal CreateUser(params Claim[] claims) => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user"), .. claims], authenticationType: "Test"));

    private static async Task<(HttpContext Context, Func<bool> NextCalled)> InvokeAsync(string headerValue,
                                                                                        ClaimsPrincipal user = null,
                                                                                        object endpointMetadata = null,
                                                                                        Action<TenantResolutionOptions> configure = null,
                                                                                        string path = "/api")
    {
        var services = new ServiceCollection();

        services.AddMultiTenancy<TestTenant, TenantId>()
                .WithResolutionStrategy<HeaderResolutionStrategy>()
                .WithStore<InMemoryTenantStore>()
                .WithResolutionOptions(opt => configure?.Invoke(opt));

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();

        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Path = path;

        if (headerValue != null)
            context.Request.Headers[HeaderResolutionStrategy.HeaderKey] = headerValue;

        if (user != null)
            context.User = user;

        if (endpointMetadata != null)
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(endpointMetadata), "test"));

        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;

        var nextCalled = false;

        var middleware = new TenantMiddleware<TestTenant, TenantId>(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.Invoke(context);

        return (context, () => nextCalled);
    }

    public class TestTenant : MilvaTenant
    {
        public TestTenant() : base()
        {
        }

        public TestTenant(string tenancyName, int branchNo) : base(tenancyName, branchNo)
        {
        }
    }

    public class InMemoryTenantStore : ITenantStore<TestTenant, TenantId>
    {
        private static readonly TestTenant[] _tenants = [new("milva", 1), new("other", 1)];

        public Task<TestTenant> GetTenantAsync(TenantId identifier) => Task.FromResult(_tenants.FirstOrDefault(t => t.Id == identifier));
    }

    #endregion
}
