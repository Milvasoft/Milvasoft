using EFCore.BulkExtensions;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Types.Structs;
using System.Linq.Expressions;

namespace Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

/// <summary>
/// Base repository for concrete repositories. All repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBaseRepository<TEntity> where TEntity : class, IMilvaEntity
{
    #region Configuration Change

    /// <summary>
    /// Determines whether save changes method called after every repository method.
    /// </summary>
    /// <param name="choice"></param>
    public void ChangeSaveChangesChoice(SaveChangesChoice choice);

    /// <summary>
    /// Resets SaveChanges choice to default.
    /// </summary>
    public void ResetSaveChangesChoiceToDefault();

    /// <summary>
    /// Determines whether soft deleted entities in the database are fetched from the database.
    /// </summary>
    /// <param name="state">Soft delete fetching state.</param>
    public void FetchSoftDeletedEntities(bool state = false);

    /// <summary>
    /// It updates the state that determines whether soft delete fetch state reset to default occurs after any fetch operation.
    /// </summary>
    /// <param name="state">Soft delete fetching reset state.</param>
    public void SoftDeleteFetchStateResetAfterOperation(bool state = false);

    /// <summary>
    /// Resets soft deleted entity fetch style to default.
    /// </summary>
    public void ResetSoftDeletedEntityFetchState();

    /// <summary>
    /// Resets soft deleted entity fetch style to default.
    /// </summary>
    public void ResetSoftDeletedEntityFetchResetState();

    #endregion

    #region Async Data Access

    #region Async FirstOrDefault 

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition = null,
                                         bool tracking = false,
                                         CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                  Expression<Func<TEntity, TResult>> projection = null,
                                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                  bool tracking = false,
                                                  CancellationToken cancellationToken = default);

    #endregion

    #region Async SingleOrDefault

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> condition = null,
                                          bool tracking = false,
                                          CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResult> GetSingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                   Expression<Func<TEntity, TResult>> projection = null,
                                                   Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                   bool tracking = false,
                                                   CancellationToken cancellationToken = default);

    #endregion

    #region Async GetById

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<TEntity> GetByIdAsync(object id,
                               Expression<Func<TEntity, bool>> conditionExpression = null,
                               bool tracking = false,
                               CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<TResult> GetByIdAsync<TResult>(object id,
                                        Expression<Func<TEntity, bool>> condition = null,
                                        Expression<Func<TEntity, TResult>> projectionExpression = null,
                                        Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                        bool tracking = false,
                                        CancellationToken cancellationToken = new CancellationToken());

    #endregion

    #region Async GetAll

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ListResponse<TEntity>> GetAllAsync(ListRequest listRequest,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            bool tracking = false,
                                            CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="listRequest"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ListResponse<TResult>> GetAllAsync<TResult>(ListRequest listRequest,
                                                     Expression<Func<TEntity, bool>> condition = null,
                                                     Expression<Func<TEntity, TResult>> projection = null,
                                                     Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                     bool tracking = false,
                                                     CancellationToken cancellationToken = default) where TResult : class;

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                    bool tracking = false,
                                    CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                             Expression<Func<TEntity, TResult>> projection = null,
                                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                             bool tracking = false,
                                             CancellationToken cancellationToken = default) where TResult : class;

    #endregion

    #region Async GetSome

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TEntity>> GetSomeAsync(int count,
                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                     bool tracking = false,
                                     CancellationToken cancellationToken = default);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TResult>> GetSomeAsync<TResult>(int count,
                                              Expression<Func<TEntity, bool>> condition = null,
                                              Expression<Func<TEntity, TResult>> projection = null,
                                              Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                              bool tracking = false,
                                              CancellationToken cancellationToken = default);

    #endregion

    #region Async Insert/Update/Delete

    /// <summary>
    ///  Adds single entity to database asynchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity,
                     CancellationToken cancellationToken = default,
                     params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    Task UpdateAsync(IEnumerable<TEntity> entities,
                     CancellationToken cancellationToken = default,
                     params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities,
                                  IEnumerable<TEntity> newEntities,
                                  CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities,
                                                           IEnumerable<TEntity> newEntities,
                                                           CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    Task RemoveAllAsync(CancellationToken cancellationToken = default);

    #region Bulk Async

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteUpdateAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteDeleteAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkAddAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BulkUpdateAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk delete operation. 
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

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default);

    #endregion

    #region Sync Data Access

    #region FirstOrDefault 

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> condition = null, bool tracking = false);

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                       Expression<Func<TEntity, TResult>> projection = null,
                                       Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                       bool tracking = false);

    #endregion

    #region SingleOrDefault

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> condition = null, bool tracking = false);

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    TResult GetSingleOrDefault<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                        Expression<Func<TEntity, TResult>> projection = null,
                                        Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                        bool tracking = false);

    #endregion

    #region GetById

    /// <summary>
    /// Returns one entity by entity Id from database synchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    TEntity GetById(object id,
                    Expression<Func<TEntity, bool>> conditionExpression = null,
                    bool tracking = false);

    /// <summary>
    /// Returns one entity by entity Id from database synchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    TResult GetById<TResult>(object id,
                             Expression<Func<TEntity, bool>> condition = null,
                             Expression<Func<TEntity, TResult>> projectionExpression = null,
                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                             bool tracking = false);

    #endregion

    #region GetAll

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    ListResponse<TEntity> GetAll(ListRequest listRequest,
                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                 bool tracking = false);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="listRequest"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    ListResponse<TResult> GetAll<TResult>(ListRequest listRequest,
                                          Expression<Func<TEntity, bool>> condition = null,
                                          Expression<Func<TEntity, TResult>> projection = null,
                                          Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                          bool tracking = false) where TResult : class;

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    List<TEntity> GetAll(Expression<Func<TEntity, bool>> conditionExpression = null, bool tracking = false);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    List<TResult> GetAll<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                  Expression<Func<TEntity, TResult>> projection = null,
                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                  bool tracking = false) where TResult : class;

    #endregion

    #region GetSome

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    List<TEntity> GetSome(int count,
                          Expression<Func<TEntity, bool>> conditionExpression = null,
                          bool tracking = false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <returns></returns>
    List<TResult> GetSome<TResult>(int count,
                                   Expression<Func<TEntity, bool>> condition = null,
                                   Expression<Func<TEntity, TResult>> projection = null,
                                   Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                   bool tracking = false);

    #endregion

    #region Insert/Update/Delete

    /// <summary>
    ///  Adds single entity to database synchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    void Add(TEntity entity);

    /// <summary>
    ///  Adds multiple entities to database synchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Updates specified entity in database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities in database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    void Update(IEnumerable<TEntity> entities);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    void Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    void Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    ///  Deletes single entity from database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    void Delete(TEntity entity);

    /// <summary>
    ///  Deletes multiple entity from database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    void Delete(IEnumerable<TEntity> entities);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    void ReplaceOldsWithNews(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    void ReplaceOldsWithNewsInSeperateDatabaseProcess(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    void RemoveAll();

    #region Bulk 

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    void ExecuteUpdate(object id, SetPropertyBuilder<TEntity> propertyBuilder);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder);

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    void ExecuteDelete(object id);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    void ExecuteDelete(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkAdd(List<TEntity> entities, Action<BulkConfig> bulkConfig = null);

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void BulkUpdate(List<TEntity> entities, Action<BulkConfig> bulkConfig = null);

    /// <summary>
    /// Bulk delete operation. 
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

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    void SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    void SaveChangesBulk(BulkConfig bulkConfig = null);

    #endregion

    /// <summary>
    /// Gets <see cref="SetPropertyBuilder{TSource}"/> for entity's matching properties with <paramref name="dto"/>'s updatable properties.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="dto"></param>
    /// <remarks>
    /// 
    /// This method is used to update the entity object with the values of the updatable properties in the DTO object.
    /// It iterates over the updatable properties in the DTO object and finds the matching property in the entity class.
    /// If a matching property is found and the property value is an instance of <see cref="IUpdateProperty"/> and IsUpdated property is true,
    /// the specified action is performed on the matching property in the entity object.
    /// 
    /// <para></para>
    /// 
    /// If entity implements <see cref="IHasModificationDate"/>, <see cref="EntityPropertyNames.LastModificationDate"/> property call will be added automatically.
    /// If entity implements <see cref="IHasModifier"/>, <see cref="EntityPropertyNames.LastModifierUserName"/> property call will be added automatically.
    /// If utc conversion requested in <see cref="DbContextConfiguration.UseUtcForDateTime"/>, <see cref="DateTime"/> typed property call will be added after converted to utc.
    /// 
    /// </remarks>
    public SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TDto>(TDto dto) where TDto : DtoBase;
}
