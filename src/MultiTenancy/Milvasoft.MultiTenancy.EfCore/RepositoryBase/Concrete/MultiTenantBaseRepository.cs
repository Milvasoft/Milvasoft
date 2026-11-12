using Milvasoft.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.EntityBases.MultiTenancy;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;

namespace Milvasoft.MultiTenancy.EfCore.RepositoryBase.Concrete;

/// <summary>
/// Base repository for concrete repositories.
/// </summary>
/// <remarks>
/// Bulk operations bypass the DbContext's save rules, so they are tenant checked here: bulk added rows are stamped with the current tenant,
/// bulk updated/deleted rows must carry the current tenant and are matched by primary key and tenant id, so rows of other tenants are never changed.
/// </remarks>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract partial class MultiTenantBaseRepository<TEntity, TContext>(TContext dbContext) : BulkBaseRepository<TEntity, TContext>(dbContext) where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaBulkDbContextBase, IMultiTenantDbContext
{
    #region Bulk Async

    /// <summary>
    /// Bulk add operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task BulkAddAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
        {
            SetTenantId(entities);

            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.CreationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.CreatorUserName);

            await _dbContext.BulkInsertAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Bulk update operation. Rows are matched by primary key and the current tenant id.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task BulkUpdateAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return Task.CompletedTask;

        EnsureBelongsToCurrentTenant(entities);

        return base.BulkUpdateAsync(entities, MatchByTenant(bulkConfig), cancellationToken);
    }

    /// <summary>
    /// Bulk delete operation. Rows are matched by primary key and the current tenant id.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task BulkDeleteAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return Task.CompletedTask;

        EnsureBelongsToCurrentTenant(entities);

        return base.BulkDeleteAsync(entities, MatchByTenant(bulkConfig), cancellationToken);
    }

    #endregion

    /// <summary>
    /// Sets the current tenant id to <paramref name="entities"/>.
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="TenantMismatchException">When there is no current tenant.</exception>
    protected internal virtual void SetTenantId(List<TEntity> entities)
    {
        if (!HasTenantId)
            return;

        var currentTenantId = GetRequiredCurrentTenantId();

        var tenantIdProp = typeof(TEntity).GetPublicPropertyIgnoreCase(nameof(TenantId));

        foreach (var entity in entities.Where(e => e != null))
            tenantIdProp.SetValue(entity, currentTenantId);
    }

    /// <summary>
    /// Verifies that every entity carries the current tenant id.
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="TenantMismatchException"></exception>
    protected internal virtual void EnsureBelongsToCurrentTenant(List<TEntity> entities)
    {
        if (!HasTenantId)
            return;

        var currentTenantId = GetRequiredCurrentTenantId();

        var tenantIdProp = typeof(TEntity).GetPublicPropertyIgnoreCase(nameof(TenantId));

        foreach (var entity in entities.Where(e => e != null))
        {
            if (tenantIdProp.GetValue(entity) is not TenantId tenantId || tenantId != currentTenantId)
                throw new TenantMismatchException($"A '{typeof(TEntity).Name}' row that does not belong to the current tenant cannot be changed.");
        }
    }

    /// <summary>
    /// Adds the tenant id to the properties bulk update/delete matches rows by (primary key by default).
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    protected Action<BulkConfig> MatchByTenant(Action<BulkConfig> bulkConfig)
    {
        if (!HasTenantId)
            return bulkConfig;

        var keyProperties = _dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(p => p.Name).ToList() ?? [];

        return config =>
        {
            bulkConfig?.Invoke(config);

            var updateByProperties = config.UpdateByProperties?.Count > 0 ? config.UpdateByProperties : keyProperties;

            config.UpdateByProperties = [.. updateByProperties.Append(EntityPropertyNames.TenantId).Distinct()];
        };
    }

    private static bool HasTenantId => CommonHelper.PropertyExists<TEntity>(nameof(TenantId));

    private TenantId GetRequiredCurrentTenantId()
    {
        var currentTenantId = _dbContext.CurrentTenantId;

        if (string.IsNullOrWhiteSpace(currentTenantId.TenancyName))
            throw new TenantMismatchException($"'{typeof(TEntity).Name}' rows cannot be written without a current tenant.");

        return currentTenantId;
    }
}
