using Milvasoft.BulkExtensions;
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
/// <remarks>
/// <para>Queries of <see cref="IHasTenantId"/> entities are filtered by <see cref="CurrentTenantId"/>; with an empty tenant they return nothing.</para>
/// <para>On save (including bulk save): added rows are stamped with <see cref="CurrentTenantId"/>; modified and deleted rows must belong to
/// <see cref="CurrentTenantId"/> (their original tenant id is checked and cannot be changed). Any violation, or writing tenant rows without a tenant,
/// throws <see cref="TenantMismatchException"/> and nothing is saved.</para>
/// <para>The tenant id of tenant entities is a concurrency token, so updates and deletes are sent with <c>WHERE ... AND "TenantId" = @currentTenant</c>.
/// An entity that was not loaded from the database (e.g. built from client input and attached with <c>Update</c>/<c>Remove</c>) and carries the key of
/// another tenant's row affects no rows, and the save fails with <see cref="DbUpdateConcurrencyException"/>.
/// This is a model change without a schema change: add an (empty) migration to update the model snapshot.</para>
/// <para>Derived contexts that override saving must override <see cref="SaveChanges(bool)"/> and <see cref="SaveChangesAsync(bool, CancellationToken)"/>.</para>
/// </remarks>
/// <param name="options"></param>
public class MilvaMultiTenancyDbContext(DbContextOptions options) : MilvaBulkDbContext(options), IMultiTenantDbContext
{
    /// <summary>
    /// Current tenant id.
    /// </summary>
    public TenantId CurrentTenantId { get; set; }

    /// <summary>
    /// Adds global query filter for tenant id and configures the tenant id as a concurrency token (see the class remarks).
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        HasTenantIdQueryFilter(modelBuilder);

        // Maps TenantId properties (UseTenantId), so the concurrency token is configured after it.
        base.OnModelCreating(modelBuilder);

        UseTenantIdAsConcurrencyToken(modelBuilder);
    }

    /// <summary>
    /// Configures the tenant id property of <see cref="IHasTenantId"/> entities as a concurrency token.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected static void UseTenantIdAsConcurrencyToken(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => e.ClrType.IsAssignableTo(typeof(IHasTenantId))))
        {
            var tenantIdProperty = entityType.FindProperty(EntityPropertyNames.TenantId);

            if (tenantIdProperty != null && (tenantIdProperty.ClrType == typeof(TenantId) || tenantIdProperty.ClrType == typeof(TenantId?)))
                tenantIdProperty.IsConcurrencyToken = true;
        }
    }

    /// <summary>
    /// Applies the tenant rules (see the class remarks) and saves.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <returns></returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantRules();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Applies the tenant rules (see the class remarks) and saves.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTenantRules();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Applies the tenant rules (see the class remarks) and saves with bulk operations.
    /// </summary>
    /// <param name="bulkConfig"></param>
    public override void SaveChangesBulk(BulkConfig bulkConfig = null)
    {
        ApplyTenantRules();

        base.SaveChangesBulk(bulkConfig);
    }

    /// <summary>
    /// Applies the tenant rules (see the class remarks) and saves with bulk operations.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        ApplyTenantRules();

        return base.SaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Stamps added tenant rows with <see cref="CurrentTenantId"/> and verifies that modified/deleted tenant rows belong to it.
    /// </summary>
    /// <exception cref="TenantMismatchException"></exception>
    protected virtual void ApplyTenantRules()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)
                || !entry.Metadata.ClrType.IsAssignableTo(typeof(IHasTenantId)))
                continue;

            var tenantIdProperty = entry.Metadata.FindProperty(EntityPropertyNames.TenantId);

            if (tenantIdProperty == null || (tenantIdProperty.ClrType != typeof(TenantId) && tenantIdProperty.ClrType != typeof(TenantId?)))
                continue;

            if (IsEmpty(CurrentTenantId))
                throw new TenantMismatchException($"'{entry.Metadata.ClrType.Name}' rows cannot be saved without a current tenant.");

            var property = entry.Property(EntityPropertyNames.TenantId);

            if (entry.State == EntityState.Added)
            {
                property.CurrentValue = CurrentTenantId;
                continue;
            }

            var originalTenantId = property.OriginalValue as TenantId?;
            var currentTenantId = property.CurrentValue as TenantId?;

            if (originalTenantId is not TenantId original || original != CurrentTenantId || currentTenantId is not TenantId current || current != CurrentTenantId)
                throw new TenantMismatchException($"A '{entry.Metadata.ClrType.Name}' row that does not belong to the current tenant cannot be {(entry.State == EntityState.Deleted ? "deleted" : "updated")}.");

            // The original tenant id equals the current tenant here. It is a concurrency token, so the update/delete only matches the row when
            // the row really belongs to the current tenant (a detached entity built with another tenant's key affects no rows).
        }
    }

    private static bool IsEmpty(TenantId tenantId) => string.IsNullOrWhiteSpace(tenantId.TenancyName);

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

/// <summary>
/// Thrown when tenant rows are written for another tenant or without a tenant.
/// </summary>
/// <param name="message"></param>
public class TenantMismatchException(string message) : InvalidOperationException(message)
{
}
