using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Utils.ExpressionVisitors;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
using Milvasoft.Types.Structs;
using System.Collections;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
/// Base repository for concrete repositories.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract partial class BaseRepository<TEntity, TContext> : IBaseRepository<TEntity> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaDbContextBase
{
    #region Protected Properties

    /// <summary>
    /// DbContext object.
    /// </summary>
    protected readonly TContext _dbContext;

    /// <summary>
    /// DebSet object.
    /// </summary>
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Data access configuration.
    /// </summary>
    protected readonly IDataAccessConfiguration _dataAccessConfiguration;

    /// <summary>
    /// It updates the state that determines whether soft delete fetch state reset occurs after any fetch operation.
    /// </summary>
    protected bool _resetSoftDeletedFetchState;

    /// <summary>
    /// Determines whether soft deleted entities in the database are fetched from the database.
    /// </summary>
    protected bool _softDeletedFetching;

    /// <summary>
    /// Determines whether save changes method called after every repository method.
    /// </summary>
    protected bool _saveChangesAfterEveryOperation;

    /// <summary>
    /// internal soft delete state.
    /// </summary>
    private bool _tempSoftDeletedFetching;

    #endregion

    /// <summary>
    /// Initializes new instance of <see cref="BaseRepository{TEntity, TContext}"/>
    /// </summary>
    /// <param name="dbContext"></param>
    protected BaseRepository(TContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();

        _dataAccessConfiguration = dbContext.GetDataAccessConfiguration();
        _softDeletedFetching = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;
        _resetSoftDeletedFetchState = _dataAccessConfiguration.Repository.ResetSoftDeletedFetchStateAfterEveryOperation;
        _saveChangesAfterEveryOperation = _dataAccessConfiguration.Repository.DefaultSaveChangesChoice == SaveChangesChoice.AfterEveryOperation;
    }

    #region Configuration Change

    /// <summary>
    /// Determines whether save changes method called after every repository method.
    /// </summary>
    /// <param name="choice"></param>
    public void ChangeSaveChangesChoice(SaveChangesChoice choice) => _saveChangesAfterEveryOperation = choice == SaveChangesChoice.AfterEveryOperation;

    /// <summary>
    /// Resets SaveChanges choice to default.
    /// </summary>
    public void ResetSaveChangesChoiceToDefault() => _saveChangesAfterEveryOperation = _dataAccessConfiguration.Repository.DefaultSaveChangesChoice == SaveChangesChoice.AfterEveryOperation;

    /// <summary>
    /// Changes soft deletion state.
    /// </summary>
    public void ChangeSoftDeletionState(SoftDeletionState state) => _dbContext.ChangeSoftDeletionState(state);

    /// <summary>
    /// Sets soft deletion state to default state in <see cref="DataAccessConfiguration"/>.
    /// </summary>
    public void SetSoftDeletionStateToDefault() => _dbContext.SetSoftDeletionStateToDefault();

    /// <summary>
    /// It updates the state that determines whether soft delete state reset to default occurs after any operation.
    /// </summary>
    /// <param name="state">Soft delete reset state.</param>
    public void SoftDeletionStateResetAfterOperation(bool state = true) => _dbContext.SoftDeletionStateResetAfterOperation(state);

    /// <summary>
    /// Determines whether soft deleted entities in the database are fetched from the database.
    /// </summary>
    /// <param name="state">Soft delete fetching state.</param>
    public void FetchSoftDeletedEntities(bool state = false) => _softDeletedFetching = state;

    /// <summary>
    /// It updates the state that determines whether soft delete fetch state reset to default occurs after any fetch operation.
    /// </summary>
    /// <param name="state">Soft delete fetching reset state.</param>
    public void SoftDeleteFetchStateResetAfterOperation(bool state = false) => _resetSoftDeletedFetchState = state;

    /// <summary>
    /// Resets soft deleted entity fetch style to default.
    /// </summary>
    public void ResetSoftDeletedEntityFetchState() => _softDeletedFetching = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;

    /// <summary>
    /// Resets soft deleted entity fetch style to default.
    /// </summary>
    public void ResetSoftDeletedEntityFetchResetState() => _resetSoftDeletedFetchState = _dataAccessConfiguration.Repository.ResetSoftDeletedFetchStateAfterEveryOperation;

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
    public virtual async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .FirstOrDefaultAsync(CreateConditionExpression(condition) ?? (entity => true), cancellationToken);

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                                       Expression<Func<TEntity, TResult>> projection = null,
                                                                       Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                       bool tracking = false,
                                                                       CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(condition) ?? (entity => true))
                       .Select(UpdateProjectionExpression(projection))
                       .FirstOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);

    #endregion

    #region Async SingleOrDefault

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> condition = null,
                                                               bool tracking = false,
                                                               CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .SingleOrDefaultAsync(CreateConditionExpression(condition) ?? (entity => true), cancellationToken);

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TResult> GetSingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                                        Expression<Func<TEntity, TResult>> projection = null,
                                                                        Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                        bool tracking = false,
                                                                        CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(condition) ?? (entity => true))
                       .Select(UpdateProjectionExpression(projection))
                       .SingleOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);

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
    public virtual async Task<TEntity> GetByIdAsync(object id,
                                                    Expression<Func<TEntity, bool>> conditionExpression = null,
                                                    bool tracking = false,
                                                    CancellationToken cancellationToken = default)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .SingleOrDefaultAsync(mainCondition, cancellationToken);
    }

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
    public virtual async Task<TResult> GetByIdAsync<TResult>(object id,
                                                             Expression<Func<TEntity, bool>> condition = null,
                                                             Expression<Func<TEntity, TResult>> projection = null,
                                                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                             bool tracking = false,
                                                             CancellationToken cancellationToken = default)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .Where(mainCondition)
                           .Select(UpdateProjectionExpression(projection))
                           .SingleOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);
    }

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
    public virtual async Task<TEntity> GetForDeleteAsync(object id,
                                                         Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                         Expression<Func<TEntity, bool>> condition = null,
                                                         bool tracking = false,
                                                         CancellationToken cancellationToken = default)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        if (includes is not null)
            return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(mainCondition)
                               .IncludeMultiple(includes)
                               .SingleOrDefaultAsync(cancellationToken);

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(mainCondition);

        return await IncludeNavigationProperties(query).SingleOrDefaultAsync(cancellationToken);
    }

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
    public virtual async Task<TResult> GetForDeleteAsync<TResult>(object id,
                                                                  Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                                  Expression<Func<TEntity, bool>> condition = null,
                                                                  Expression<Func<TEntity, TResult>> projection = null,
                                                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                  bool tracking = false,
                                                                  CancellationToken cancellationToken = default)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        if (includes is not null)
            return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(mainCondition)
                               .IncludeMultiple(includes)
                               .Select(UpdateProjectionExpression(projection))
                               .SingleOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(mainCondition);

        return await IncludeNavigationProperties(query).Select(UpdateProjectionExpression(projection))
                                                       .SingleOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);
    }

    /// <summary>
    /// Returns entities from database asynchronously for delete with navigation properties.
    /// If you don't send <paramref name="includes"/>, <see cref="CascadeOnDeleteAttribute"/> marked properties will include.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="condition"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    public virtual async Task<List<TEntity>> GetForDeleteAsync(Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                         Expression<Func<TEntity, bool>> condition = null,
                                                         bool tracking = false,
                                                         CancellationToken cancellationToken = default)
    {
        if (includes is not null)
            return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(CreateConditionExpression(condition) ?? (entity => true))
                               .IncludeMultiple(includes)
                               .ToListAsync(cancellationToken);

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(CreateConditionExpression(condition) ?? (entity => true));

        return await IncludeNavigationProperties(query).ToListAsync(cancellationToken);
    }

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
    public virtual async Task<List<TResult>> GetForDeleteAsync<TResult>(Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                                        Expression<Func<TEntity, bool>> condition = null,
                                                                        Expression<Func<TEntity, TResult>> projection = null,
                                                                        Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                        bool tracking = false,
                                                                        CancellationToken cancellationToken = default)
    {
        if (includes is not null)
            return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(CreateConditionExpression(condition) ?? (entity => true))
                               .IncludeMultiple(includes)
                               .Select(UpdateProjectionExpression(projection))
                               .ToListAsync(cancellationToken);

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(CreateConditionExpression(condition) ?? (entity => true));

        return await IncludeNavigationProperties(query).Select(UpdateProjectionExpression(projection))
                                                       .Where(CreateConditionExpression(conditionAfterProjection) ?? (entity => true))
                                                       .ToListAsync(cancellationToken);
    }

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
    public virtual async Task<ListResponse<TEntity>> GetAllAsync(ListRequest listRequest,
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .ToListResponseAsync(listRequest, cancellationToken);

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
    public virtual async Task<ListResponse<TResult>> GetAllAsync<TResult>(ListRequest listRequest,
                                                                          Expression<Func<TEntity, bool>> condition = null,
                                                                          Expression<Func<TEntity, TResult>> projection = null,
                                                                          Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                          bool tracking = false,
                                                                          CancellationToken cancellationToken = default) where TResult : class
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(condition) ?? (entity => true))
                       .Select(UpdateProjectionExpression(projection))
                       .Where(conditionAfterProjection ?? (entity => true))
                       .ToListResponseAsync(listRequest, cancellationToken);

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                         bool tracking = false,
                                                         CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .ToListAsync(cancellationToken);

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
    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                                  Expression<Func<TEntity, TResult>> projection = null,
                                                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                  bool tracking = false,
                                                                  CancellationToken cancellationToken = default) where TResult : class
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(condition) ?? (entity => true))
                       .Select(UpdateProjectionExpression(projection))
                       .Where(conditionAfterProjection ?? (entity => true))
                       .ToListAsync(cancellationToken);

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
    public virtual async Task<List<TEntity>> GetSomeAsync(int count,
                                                          Expression<Func<TEntity, bool>> conditionExpression = null,
                                                          bool tracking = false,
                                                          CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Take(count)
                       .ToListAsync(cancellationToken);

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
    public virtual async Task<List<TResult>> GetSomeAsync<TResult>(int count,
                                                                   Expression<Func<TEntity, bool>> condition = null,
                                                                   Expression<Func<TEntity, TResult>> projection = null,
                                                                   Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                                   bool tracking = false,
                                                                   CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(condition) ?? (entity => true))
                       .Select(UpdateProjectionExpression(projection))
                       .Where(conditionAfterProjection ?? (entity => true))
                       .Take(count)
                       .ToListAsync(cancellationToken);

    #endregion

    #region Async Insert/Update/Delete

    /// <summary>
    ///  Adds single entity to database asynchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        _dbSet.AddRange(entities);

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Update(entity);

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Update(entity);
        }

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual async Task<int> UpdateAsync(TEntity entity,
                                               CancellationToken cancellationToken = default,
                                               params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (propertySelectors.IsNullOrEmpty())
            return 0;

        DetachFromLocalIfExists(entity);

        var entry = _dbSet.Entry(entity);

        foreach (var includeProperty in propertySelectors)
            entry.Property(includeProperty).IsModified = true;

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual async Task<int> UpdateAsync(IEnumerable<TEntity> entities,
                                               CancellationToken cancellationToken = default,
                                               params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        if (entities.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            return 0;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            var entry = _dbSet.Entry(entity);

            foreach (var includeProperty in propertySelectors)
                entry.Property(includeProperty).IsModified = true;
        }

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Remove(entity);

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities,
                                                            IEnumerable<TEntity> newEntities,
                                                            CancellationToken cancellationToken = default)
    {
        if (!oldEntities.IsNullOrEmpty())
            foreach (var entity in oldEntities)
            {
                DetachFromLocalIfExists(entity);
                _dbSet.Remove(entity);
            }

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);

        return await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities,
                                                                                     IEnumerable<TEntity> newEntities,
                                                                                     CancellationToken cancellationToken = default)
    {
        int affectedRows = 0;

        if (!oldEntities.IsNullOrEmpty())
            affectedRows += await DeleteAsync(oldEntities, cancellationToken);

        if (!newEntities.IsNullOrEmpty())
            affectedRows += await AddRangeAsync(newEntities, cancellationToken);

        return affectedRows;
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = _dbSet.AsEnumerable();

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        return await InternalSaveChangesAsync(cancellationToken);
    }

    #region Bulk Async

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ExecuteUpdateAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
        => await ExecuteUpdateAsync(i => i.Id == id, propertyBuilder, cancellationToken);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
    {
        if (!propertyBuilder.AuditCallsAdded)
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);
        }

        return await _dbSet.Where(predicate).ExecuteUpdateAsync(propertyBuilder.SetPropertyCalls, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ExecuteDeleteAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder = null, CancellationToken cancellationToken = default)
        => await ExecuteDeleteAsync(i => i.Id == id, propertyBuilder, cancellationToken: cancellationToken);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate,
                                                      SetPropertyBuilder<TEntity> propertyBuilder = null,
                                                      CancellationToken cancellationToken = default)
    {
        // If soft deletion active and entity is soft deletable.
        if (_dbContext.GetCurrentSoftDeletionState() == SoftDeletionState.Active && CommonHelper.PropertyExists<TEntity>(EntityPropertyNames.IsDeleted))
        {
            propertyBuilder ??= new SetPropertyBuilder<TEntity>();

            //Soft delete
            AddDeletionPropertyCalls(propertyBuilder);

            return await _dbSet.Where(predicate).ExecuteUpdateAsync(propertyBuilder.SetPropertyCalls, cancellationToken: cancellationToken);
        }

        return await _dbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }

    #endregion

    #endregion

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
    public virtual SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TDto>(TDto dto) where TDto : DtoBase
        => _dbContext.GetUpdatablePropertiesBuilder<TEntity, TDto>(dto);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _dbContext.SaveChangesAsync(cancellationToken);

    #region Private Helper Methods

    /// <summary>
    /// Detach entity if found in local store.
    /// </summary>
    /// <param name="entity"></param>
    protected virtual void DetachFromLocalIfExists(TEntity entity)
    {
        var localEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(u => u.GetUniqueIdentifier().Equals(entity.GetUniqueIdentifier()));

        if (localEntity != null)
        {
            var entry = _dbContext.Entry(localEntity);
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Creates Id == <paramref name="key"/> equality expression and append to <see cref="CommonHelper.CreateIsDeletedFalseExpression"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TEntity, bool>> CreateKeyEqualityExpressionWithIsDeletedFalse(object key, Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        Expression<Func<TEntity, bool>> idCondition = i => i.Id.Equals(key);

        var mainCondition = idCondition.Append(CreateConditionExpression(conditionExpression) ?? (entity => true), ExpressionType.AndAlso);

        return mainCondition.Append(conditionExpression, ExpressionType.AndAlso);
    }

    /// <summary>
    /// If <see cref="_softDeletedFetching"/> is false,  appends is deleted false expression to <paramref name="conditionExpression"/>.
    /// Else does nothing to <paramref name="conditionExpression"/> but if <see cref="_resetSoftDeletedFetchState"/> is true then sets <see cref="_softDeletedFetching"/> false.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected virtual Expression<Func<T, bool>> CreateConditionExpression<T>(Expression<Func<T, bool>> conditionExpression = null)
    {
        Expression<Func<T, bool>> mainExpression;

        _tempSoftDeletedFetching = _softDeletedFetching;

        //Step in when _softDeletedFetching is false
        if (!_tempSoftDeletedFetching)
        {
            var softDeleteExpression = CommonHelper.CreateIsDeletedFalseExpression<T>();

            mainExpression = softDeleteExpression.Append(conditionExpression, ExpressionType.AndAlso);
        }
        else
        {
            mainExpression = conditionExpression;
        }

        if (_resetSoftDeletedFetchState)
            _softDeletedFetching = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;

        return mainExpression;
    }

    /// <summary>
    /// Updates projection according to soft delete.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TEntity, TResult>> UpdateProjectionExpression<TResult>(Expression<Func<TEntity, TResult>> projectionExpression = null)
    {
        Expression<Func<TEntity, TResult>> mainExpression;

        if (!_tempSoftDeletedFetching)
        {
            var appendedExpression = AppendSoftDeleteFilterToProjection(projectionExpression);

            mainExpression = appendedExpression;
        }
        else
        {
            mainExpression = projectionExpression;
        }

        if (_resetSoftDeletedFetchState)
            _softDeletedFetching = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;

        return mainExpression;
    }

    /// <summary>
    /// Returns <see cref="QueryTrackingBehavior"/> according to <paramref name="tracking"/>.
    /// </summary>
    /// <param name="tracking"></param>
    /// <returns></returns>
    protected static QueryTrackingBehavior GetQueryTrackingBehavior(bool tracking) => tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTrackingWithIdentityResolution;

    /// <summary>
    /// Save changes according to <see cref="_saveChangesAfterEveryOperation"/>.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> InternalSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_saveChangesAfterEveryOperation)
            return await _dbContext.SaveChangesAsync(cancellationToken);

        return 0;
    }

    #region Auditing

    /// <summary>
    /// Adds perform time, performer and IsDeleted proeperty calls to <paramref name="propertyBuilder"/>. 
    /// </summary>
    /// <param name="propertyBuilder"></param>
    protected internal virtual void AddDeletionPropertyCalls(SetPropertyBuilder<TEntity> propertyBuilder)
    {
        if (_dataAccessConfiguration.Auditing.AuditDeletionDate)
            AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.DeletionDate);

        var isDeletedPropertyExpression = CommonHelper.CreatePropertySelector<TEntity, bool>(EntityPropertyNames.IsDeleted);

        propertyBuilder.SetPropertyValue(isDeletedPropertyExpression, true);

        if (_dataAccessConfiguration.Auditing.AuditDeleter)
            AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.DeleterUserName);
    }

    /// <summary>
    /// Adds perform time property calls to <paramref name="propertyBuilder"/>.
    /// </summary>
    /// <param name="propertyBuilder"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void AddPerformTimePropertyCall(SetPropertyBuilder<TEntity> propertyBuilder, string propertyName)
    {
        if (CommonHelper.PropertyExists<TEntity>(propertyName))
        {
            var performTimePropertyExpression = CommonHelper.CreatePropertySelector<TEntity, DateTime>(propertyName);

            var now = CommonHelper.GetNow(_dataAccessConfiguration.DbContext.UseUtcForDateTime);

            propertyBuilder.SetPropertyValue(performTimePropertyExpression, now);
        }
    }

    /// <summary>
    /// Adds performer user property calls to <paramref name="propertyBuilder"/>.
    /// </summary>
    /// <param name="propertyBuilder"></param>
    /// <param name="propertyName"></param>
    protected internal virtual void AddPerformerUserPropertyCall(SetPropertyBuilder<TEntity> propertyBuilder, string propertyName)
    {
        if (CommonHelper.PropertyExists<TEntity>(propertyName))
        {
            var currentUserName = _dataAccessConfiguration.DbContext.InvokeGetCurrentUserMethod(_dbContext.ServiceProvider);

            if (!string.IsNullOrWhiteSpace(currentUserName))
            {
                var performerUserNamePropertyExpression = CommonHelper.CreatePropertySelector<TEntity, string>(propertyName);

                propertyBuilder.SetPropertyValue(performerUserNamePropertyExpression, currentUserName);
            }
        }
    }

    /// <summary>
    /// Includes <see cref="CascadeOnDeleteAttribute"/> marked navigation properties to query.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected static IQueryable<TEntity> IncludeNavigationProperties(IQueryable<TEntity> query)
    {
        var cascadePropertyPaths = GetCascadePropertyPaths<TEntity>();

        var duplications = new List<string>();

        for (int i = 0; i < cascadePropertyPaths.Count; i++)
        {
            var cascadePropertyPath = cascadePropertyPaths[i];

            if (cascadePropertyPaths.Exists(c => c != cascadePropertyPath && c.StartsWith(cascadePropertyPath)))
                duplications.Add(cascadePropertyPath);
        }

        foreach (var cascadeProp in cascadePropertyPaths)
        {
            if (duplications.Contains(cascadeProp))
                continue;

            query = query.Include(cascadeProp);
        }

        return query;
    }

    /// <summary>
    /// Gets property paths.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parentPath"></param>
    /// <param name="maxDepth"></param>
    /// <returns></returns>
    protected static List<string> GetCascadePropertyPaths<T>(string parentPath = null, int maxDepth = 5) => GetCascadePropertyPaths(typeof(T), parentPath, maxDepth);

    /// <summary>
    /// Gets property paths.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parentPath"></param>
    /// <param name="maxDepth"></param>
    /// <param name="currentDepth"></param>
    /// <returns></returns>
    protected static List<string> GetCascadePropertyPaths(Type type, string parentPath = null, int maxDepth = 5, int currentDepth = 0)
    {
        var properties = type.GetProperties()
                             .Where(p => p.IsDefined(typeof(CascadeOnDeleteAttribute), inherit: false))
                             .ToList();

        var paths = new List<string>();

        if (currentDepth > maxDepth)
            return paths;

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;

            if (!propertyType.IsClass || propertyType == typeof(string) || propertyType == typeof(decimal))
                continue;

            var currentPath = string.IsNullOrEmpty(parentPath) ? property.Name : $"{parentPath}.{property.Name}";

            paths.Add(currentPath);

            // Eğer property bir sınıfsa ve string değilse, alt property'leri rekrüzyonla getir
            if (propertyType.IsGenericType && typeof(IList).IsAssignableFrom(propertyType))
            {
                var genericArgumentType = propertyType.GetGenericArguments()[0];

                paths.AddRange(GetCascadePropertyPaths(genericArgumentType, currentPath, maxDepth, currentDepth + 1));
            }
        }

        return paths;
    }

    private static Expression<Func<TEntity, TResult>> AppendSoftDeleteFilterToProjection<TResult>(Expression<Func<TEntity, TResult>> projection)
    {
        if (projection is null)
            return null;

        var visitor = new SoftDeleteFilterVisitor();

        var newBody = visitor.Visit(projection.Body);

        return Expression.Lambda<Func<TEntity, TResult>>(newBody, projection.Parameters);
    }

    #endregion

    #endregion
}
