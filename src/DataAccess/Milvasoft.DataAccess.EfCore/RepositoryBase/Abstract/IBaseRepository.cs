using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
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
    /// Changes soft deletion state.
    /// </summary>
    public void ChangeSoftDeletionState(SoftDeletionState state);

    /// <summary>
    /// Sets soft deletion state to default state in <see cref="DataAccessConfiguration"/>.
    /// </summary>
    public void SetSoftDeletionStateToDefault();

    /// <summary>
    /// It updates the state that determines whether soft delete state reset to default occurs after any operation.
    /// </summary>
    /// <param name="state">Soft delete reset state.</param>
    public void SoftDeletionStateResetAfterOperation(bool state = true);

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
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<TResult> GetByIdAsync<TResult>(object id,
                                        Expression<Func<TEntity, bool>> condition = null,
                                        Expression<Func<TEntity, TResult>> projection = null,
                                        Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                        bool tracking = false,
                                        CancellationToken cancellationToken = new CancellationToken());

    #endregion

    #region GetForDeleteAsync

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<TEntity> GetForDeleteAsync(object id,
                                    Func<IIncludable<TEntity>, IIncludable> includes = null,
                                    Expression<Func<TEntity, bool>> condition = null,
                                    bool tracking = false,
                                    CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<TResult> GetForDeleteAsync<TResult>(object id,
                                             Func<IIncludable<TEntity>, IIncludable> includes = null,
                                             Expression<Func<TEntity, bool>> condition = null,
                                             Expression<Func<TEntity, TResult>> projection = null,
                                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                             bool tracking = false,
                                             CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Returns entities from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<List<TEntity>> GetForDeleteAsync(Func<IIncludable<TEntity>, IIncludable> includes = null,
                                          Expression<Func<TEntity, bool>> condition = null,
                                          bool tracking = false,
                                          CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns entities from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    Task<List<TResult>> GetForDeleteAsync<TResult>(Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                   Expression<Func<TEntity, bool>> condition = null,
                                                   Expression<Func<TEntity, TResult>> projection = null,
                                                   Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                   bool tracking = false,
                                                   CancellationToken cancellationToken = default);

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
    Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(TEntity entity,
                     CancellationToken cancellationToken = default,
                     params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(IEnumerable<TEntity> entities,
                     CancellationToken cancellationToken = default,
                     params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities,
                                  IEnumerable<TEntity> newEntities,
                                  CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities,
                                                           IEnumerable<TEntity> newEntities,
                                                           CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    Task<int> RemoveAllAsync(CancellationToken cancellationToken = default);

    #region Bulk Async

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteUpdateAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteDeleteAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate,
                            SetPropertyBuilder<TEntity> propertyBuilder = null,
                            CancellationToken cancellationToken = default);

    #endregion

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

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
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    TResult GetById<TResult>(object id,
                             Expression<Func<TEntity, bool>> condition = null,
                             Expression<Func<TEntity, TResult>> projection = null,
                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                             bool tracking = false);

    #endregion

    #region GetForDelete

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    TEntity GetForDelete(object id,
                         Func<IIncludable<TEntity>, IIncludable> includes = null,
                         Expression<Func<TEntity, bool>> condition = null,
                         bool tracking = false);

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    TResult GetForDelete<TResult>(object id,
                                  Func<IIncludable<TEntity>, IIncludable> includes = null,
                                  Expression<Func<TEntity, bool>> condition = null,
                                  Expression<Func<TEntity, TResult>> projection = null,
                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                  bool tracking = false);

    /// <summary>
    /// Returns entities from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    List<TEntity> GetForDelete(Func<IIncludable<TEntity>, IIncludable> includes = null,
                               Expression<Func<TEntity, bool>> condition = null,
                               bool tracking = false);

    /// <summary>
    /// Returns entities from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    List<TResult> GetForDelete<TResult>(Func<IIncludable<TEntity>, IIncludable> includes = null,
                                        Expression<Func<TEntity, bool>> condition = null,
                                        Expression<Func<TEntity, TResult>> projection = null,
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
    int Add(TEntity entity);

    /// <summary>
    ///  Adds multiple entities to database synchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    int AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Updates specified entity in database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    int Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities in database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    int Update(IEnumerable<TEntity> entities);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    int Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    int Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] propertySelectors);

    /// <summary>
    ///  Deletes single entity from database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    int Delete(TEntity entity);

    /// <summary>
    ///  Deletes multiple entity from database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    int Delete(IEnumerable<TEntity> entities);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    int ReplaceOldsWithNews(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    int ReplaceOldsWithNewsInSeperateDatabaseProcess(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    int RemoveAll();

    #region Bulk 

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    int ExecuteUpdate(object id, SetPropertyBuilder<TEntity> propertyBuilder);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    int ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder);

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <returns></returns>
    int ExecuteDelete(object id, SetPropertyBuilder<TEntity> propertyBuilder = null);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <returns></returns>
    int ExecuteDelete(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder = null);

    #endregion

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    int SaveChanges();

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
