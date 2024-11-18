using EFCore.BulkExtensions;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

namespace Milvasoft.DataAccess.EfCore.Bulk.RepositoryBase.Abstract;

/// <summary>
/// Base repository for concrete repositories. All repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBulkBaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IMilvaEntity
{
    #region Bulk Async

    /// <summary>
    /// Bulk add operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkAddAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkUpdateAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk delete operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkDeleteAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkAddWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkUpdateWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkDeleteWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default);

    #region Bulk 

    /// <summary>
    /// Bulk add operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkAdd(List<TEntity> entities, Action<BulkConfig> bulkConfig = null);

    /// <summary>
    /// Bulk update operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkUpdate(List<TEntity> entities, Action<BulkConfig> bulkConfig = null);

    /// <summary>
    /// Bulk delete operation. This method will not save changes to the database. So soft delete and auditing operations will not be performed.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkDelete(List<TEntity> entities, Action<BulkConfig> bulkConfig = null);

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkAddWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null);

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkUpdateWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null);

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkDeleteWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null);

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null);
}
