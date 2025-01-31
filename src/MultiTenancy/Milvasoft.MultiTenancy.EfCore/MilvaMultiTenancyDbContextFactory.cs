using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.MultiTenancy.ResolutionStrategy;

namespace Milvasoft.MultiTenancy.EfCore;

/// <summary>
/// Milva multi tenancy db context scoped factory. Sets the resolution strategy and data access configuration when creating db context.
/// </summary>
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

        var tenantResolutionStrategy = serviceProvider.GetService<ITenantResolutionStrategy<TenantId>>();
        context.CurrentTenantId = tenantResolutionStrategy.GetTenantIdentifier();
        context.ServiceProvider = serviceProvider;
        context.SetDataAccessConfiguration(dataAccessConfiguration);

        return context;
    }
}
