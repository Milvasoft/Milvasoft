using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Exceptions;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.MultiTenancy.Context;

namespace Milvasoft.MultiTenancy.EfCore;

/// <summary>
/// Milva multi tenancy db context scoped factory. Sets the current tenant and data access configuration when creating db context.
/// </summary>
/// <remarks>
/// The tenant is read only from the scoped <see cref="ITenantContext"/> that the tenant middleware (or the application, for background work) fills
/// after validating it. The raw request (header, host) is never read here. When no tenant is resolved, <see cref="MilvaMultiTenancyDbContext.CurrentTenantId"/>
/// is <see cref="TenantId.Empty"/>: tenant queries return nothing and tenant writes throw until a tenant is set.
/// </remarks>
/// <param name="pooledFactory"></param>
/// <param name="dataAccessConfiguration"></param>
/// <param name="serviceProvider"></param>
public class MilvaMultiTenancyDbContextFactory<TContext>(IDbContextFactory<TContext> pooledFactory,
                                                         IDataAccessConfiguration dataAccessConfiguration,
                                                         IServiceProvider serviceProvider) : IDbContextFactory<TContext> where TContext : MilvaMultiTenancyDbContext
{
    /// <summary>
    /// Db context creation implementation.
    /// </summary>
    /// <returns></returns>
    public TContext CreateDbContext()
    {
        var context = pooledFactory.CreateDbContext();

        // Always assign: pooled contexts keep the value of their previous lease.
        context.CurrentTenantId = GetCurrentTenantId();
        context.ServiceProvider = serviceProvider;
        context.SetDataAccessConfiguration(dataAccessConfiguration);

        return context;
    }

    private TenantId GetCurrentTenantId()
    {
        var tenantContext = serviceProvider.GetService<ITenantContext>();

        if (tenantContext?.IsResolved != true)
            return TenantId.Empty;

        return tenantContext.TenantIdentifier as TenantId?
               ?? throw new MilvaDeveloperException($"{nameof(MilvaMultiTenancyDbContext)} requires a {nameof(TenantId)} tenant identifier but the tenant context holds '{tenantContext.TenantIdentifier?.GetType().Name}'.");
    }
}
