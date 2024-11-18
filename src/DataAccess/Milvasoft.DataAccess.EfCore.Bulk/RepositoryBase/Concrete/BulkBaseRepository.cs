using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore.Bulk;

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
            await _dbContext.BulkInsertAsync(entities, bulkConfig, cancellationToken: cancellationToken);
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
            await _dbContext.BulkUpdateAsync(entities, bulkConfig, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Bulk delete operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkDeleteAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            await _dbContext.BulkDeleteAsync(entities, bulkConfig, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkAddWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.AddRange(entities);

        await InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkUpdateWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.UpdateRange(entities);

        await InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task BulkDeleteWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        await InternalSaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) => await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken);

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
}
