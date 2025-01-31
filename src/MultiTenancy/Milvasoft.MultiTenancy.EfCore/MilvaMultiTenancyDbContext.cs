using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using System.Linq.Expressions;

namespace Milvasoft.MultiTenancy.EfCore;

/// <summary>
/// Defines the tenant id type.
/// </summary>
public interface IMultiTenantDbContext : IMilvaBulkDbContextBase
{
    /// <summary>
    /// Current tenant id.  
    /// </summary>
    public TenantId CurrentTenantId { get; set; }
}

/// <summary>
/// Db context base for single database multi tenancy scenarios.
/// </summary>
/// <param name="options"></param>
public class MilvaMultiTenancyDbContext(DbContextOptions options) : MilvaBulkDbContext(options), IMultiTenantDbContext
{
    /// <summary>
    /// Current tenant id.  
    /// </summary>
    public TenantId CurrentTenantId { get; set; }

    /// <summary>
    /// Adds global query filter for tenant id.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        HasTenantIdQueryFilter(modelBuilder);

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
                entry.Property(EntityPropertyNames.TenantId).CurrentValue = CurrentTenantId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void HasTenantIdQueryFilter(ModelBuilder modelBuilder)
    {
        var tenantEntities = modelBuilder.Model.GetEntityTypes().Where(entityType => typeof(IHasTenantId).IsAssignableFrom(entityType.ClrType)).Select(e => e.ClrType);

        foreach (var clrType in tenantEntities)
        {
            var parameter = Expression.Parameter(clrType, "entity");

            // Find the property that matches the tenant ID property name
            var property = clrType.GetProperty(EntityPropertyNames.TenantId);

            if (property == null)
                continue;

            // Create the expression: entity.TenantId == CurrentTenantId
            var propertyAccess = Expression.Property(parameter, property);
            var tenantIdValue = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
            var filterExpression = Expression.Equal(propertyAccess, tenantIdValue);

            var lambda = Expression.Lambda(filterExpression, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}
