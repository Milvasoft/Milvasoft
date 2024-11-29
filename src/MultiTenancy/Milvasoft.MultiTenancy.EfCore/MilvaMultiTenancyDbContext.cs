using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.MultiTenancy.ResolutionStrategy;

namespace Milvasoft.MultiTenancy.EfCore;

/// <summary>
/// Db context base for single database multi tenancy scenarios.
/// </summary>
/// <param name="options"></param>
public class MilvaMultiTenancyDbContext(DbContextOptions options) : MilvaBulkDbContext(options)
{
    /// <summary>
    /// Tenant resolution strategy.
    /// </summary>
    public ITenantResolutionStrategy<TenantId> TenantResolutionStrategy { get; set; }

    /// <summary>
    /// Adds global query filter for tenant id.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseTenantIdQueryFilter(TenantResolutionStrategy.GetTenantIdentifier());

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Sets the tenant id for the entity that implements <see cref="IHasTenantId"/> when insert.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.Metadata.ClrType.IsAssignableTo(typeof(IHasTenantId))))
        {
            if (entry.State == EntityState.Added)
                entry.Property(EntityPropertyNames.TenantId).CurrentValue = TenantResolutionStrategy.GetTenantIdentifier();
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
