using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Helpers;
using Milvasoft.DataAccess.EfCore.Bulk;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories.
/// </summary>
public abstract partial class BulkBaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaBulkDbContextBase
{
    #region Bulk 

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void BulkAdd(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbContext.BulkInsert(entities, bulkConfig);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void BulkUpdate(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbContext.BulkUpdate(entities, bulkConfig);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void BulkDelete(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbContext.BulkDelete(entities, bulkConfig);
    }

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void BulkAddWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
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
    public virtual void BulkUpdateWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
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
    public virtual void BulkDeleteWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        InternalSaveChangesBulk(bulkConfig);
    }

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public void SaveChangesBulk(BulkConfig bulkConfig = null) => _dbContext.SaveChangesBulk(bulkConfig);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    protected void InternalSaveChangesBulk(BulkConfig bulkConfig = null)
    {
        if (_saveChangesAfterEveryOperation)
            _dbContext.SaveChangesBulk(bulkConfig);
    }
}
