using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk;
using Milvasoft.DataAccess.EfCore.Utils.Enums;

namespace Milvasoft.MultiTenancy.EfCore.RepositoryBase.Concrete;

/// <summary>
///  Base repository for concrete repositories.
/// </summary>
public abstract partial class MultiTenantBaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaBulkDbContextBase, IMultiTenantDbContext
{
    #region Bulk 

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkAdd(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
        {
            SetTenantId(entities);

            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.CreationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.CreatorUserName);

            _dbContext.BulkInsert(entities, bulkConfig);
        }
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkUpdate(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
        {
            SetTenantId(entities);

            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.LastModifierUserName);

            _dbContext.BulkUpdate(entities, bulkConfig);
        }
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkDelete(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
        {
            SetTenantId(entities);

            // If soft deletion active and entity is soft deletable.
            if (_dbContext.GetCurrentSoftDeletionState() == SoftDeletionState.Active && CommonHelper.PropertyExists<TEntity>(EntityPropertyNames.IsDeleted))
            {
                //Soft delete
                SetDeletionValues(entities);

                BulkUpdate(entities, bulkConfig);
                return;
            }

            _dbContext.BulkDelete(entities, bulkConfig);
        }
    }

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkAddWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.AddRange(entities);

        InternalSaveChangesBulk(bulkConfig);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkUpdateWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.UpdateRange(entities);

        InternalSaveChangesBulk(bulkConfig);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public override void BulkDeleteWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        InternalSaveChangesBulk(bulkConfig);
    }

    #endregion
}
