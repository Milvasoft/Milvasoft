using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.Bulk;
using Milvasoft.DataAccess.EfCore.Utils.Enums;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
/// Base repository for concrete repositories.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract partial class BulkBaseRepository<TEntity, TContext>(TContext dbContext) : BaseRepository<TEntity, TContext>(dbContext) where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaBulkDbContextBase
{
    #region Bulk Async

    /// <summary>
    /// Bulk add operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkAddAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.CreationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.CreatorUserName);

            await _dbContext.BulkInsertAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Bulk update operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkUpdateAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                SetPerformTimeValues(entities, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                SetPerformerUserValues(entities, EntityPropertyNames.LastModifierUserName);

            await _dbContext.BulkUpdateAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Bulk delete operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed. 
    /// <see cref="CascadeOnDeleteAttribute"/> will not be considered on real delete operations when soft delete is passive.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkDeleteAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
        {
            // If soft deletion active and entity is soft deletable.
            if (_dbContext.GetCurrentSoftDeletionState() == SoftDeletionState.Active && CommonHelper.PropertyExists<TEntity>(EntityPropertyNames.IsDeleted))
            {
                //Soft delete
                SetDeletionValues(entities);

                await BulkUpdateAsync(entities, bulkConfig, cancellationToken);

                return;
            }

            await _dbContext.BulkDeleteAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task BulkAddWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.AddRange(entities);

        return InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task BulkUpdateWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.UpdateRange(entities);

        return InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task BulkDeleteWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        return InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) => _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken);

    /// <summary>
    /// Save changes according to save changes choise.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task InternalSaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (_saveChangesAfterEveryOperation)
            await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Sets perform time, performer and IsDeleted values to <paramref name="entities"/>.
    /// </summary>
    /// <param name="entities"></param>
    protected internal virtual void SetDeletionValues(List<TEntity> entities)
    {
        if (_dataAccessConfiguration.Auditing.AuditDeletionDate)
            SetPerformTimeValues(entities, EntityPropertyNames.DeletionDate);

        var isDeletedProp = typeof(TEntity).GetPublicPropertyIgnoreCase(EntityPropertyNames.IsDeleted);

        foreach (var entity in entities)
            isDeletedProp.SetValue(entity, true);

        if (_dataAccessConfiguration.Auditing.AuditDeleter)
            SetPerformerUserValues(entities, EntityPropertyNames.DeleterUserName);
    }

    /// <summary>
    /// Sets perform time values to <paramref name="entities"/>.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void SetPerformTimeValues(List<TEntity> entities, string propertyName)
    {
        if (CommonHelper.PropertyExists<TEntity>(propertyName))
        {
            var performTimeProp = typeof(TEntity).GetPublicPropertyIgnoreCase(propertyName);

            var now = CommonHelper.GetNow(_dataAccessConfiguration.DbContext.UseUtcForDateTime);

            foreach (var entity in entities)
                performTimeProp.SetValue(entity, now);
        }
    }

    /// <summary>
    /// Sets performer user values to <paramref name="entities"/>.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void SetPerformerUserValues(List<TEntity> entities, string propertyName)
    {
        if (CommonHelper.PropertyExists<TEntity>(propertyName))
        {
            var performerUserProp = typeof(TEntity).GetPublicPropertyIgnoreCase(propertyName);

            var currentUserName = _dataAccessConfiguration.DbContext.InvokeGetCurrentUserMethod(_dbContext.ServiceProvider);

            foreach (var entity in entities)
                if (!string.IsNullOrWhiteSpace(currentUserName))
                    performerUserProp.SetValue(entity, currentUserName);
        }
    }
}
