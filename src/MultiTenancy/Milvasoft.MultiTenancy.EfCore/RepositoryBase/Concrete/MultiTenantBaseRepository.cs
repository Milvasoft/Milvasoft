using EFCore.BulkExtensions;
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

    #endregion

    /// <summary>
    /// Sets performer user values to <paramref name="entities"/>.
    /// </summary>
    /// <param name="entities"></param>
    protected internal virtual void SetTenantId(List<TEntity> entities)
    {
        if (CommonHelper.PropertyExists<TEntity>(nameof(TenantId)))
        {
            var tenantIdProp = typeof(TEntity).GetPublicPropertyIgnoreCase(nameof(TenantId));

            var currentTenantId = _dbContext.TenantResolutionStrategy.GetTenantIdentifier();

            foreach (var entity in entities)
                tenantIdProp.SetValue(entity, currentTenantId);
        }
    }
}
