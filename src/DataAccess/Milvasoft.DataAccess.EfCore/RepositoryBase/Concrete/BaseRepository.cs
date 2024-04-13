using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.Types.Structs;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
/// Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract partial class BaseRepository<TEntity, TContext> : IBaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaDbContextBase
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
                       .Select(projection)
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
                       .Select(projection)
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
                                                    CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .SingleOrDefaultAsync(mainCondition, cancellationToken);
    }

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
    public virtual async Task<TResult> GetByIdAsync<TResult>(object id,
                                                             Expression<Func<TEntity, bool>> condition = null,
                                                             Expression<Func<TEntity, TResult>> projectionExpression = null,
                                                             Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                             bool tracking = false,
                                                             CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, condition);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .Where(mainCondition)
                           .Select(projectionExpression)
                           .SingleOrDefaultAsync(conditionAfterProjection ?? (entity => true), cancellationToken);
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
                       .Select(projection)
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
                       .Select(projection)
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
                       .Select(projection)
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
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return;

        _dbSet.AddRange(entities);

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Update(entity);

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Update(entity);
        }

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity,
                                          CancellationToken cancellationToken = default,
                                          params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (propertySelectors.IsNullOrEmpty())
            return;

        DetachFromLocalIfExists(entity);

        var entry = _dbSet.Entry(entity);

        foreach (var includeProperty in propertySelectors)
            entry.Property(includeProperty).IsModified = true;

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(IEnumerable<TEntity> entities,
                                          CancellationToken cancellationToken = default,
                                          params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        if (entities.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            var entry = _dbSet.Entry(entity);

            foreach (var includeProperty in propertySelectors)
                entry.Property(includeProperty).IsModified = true;
        }

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Remove(entity);

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities,
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

        await InternalSaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities,
                                                                                IEnumerable<TEntity> newEntities,
                                                                                CancellationToken cancellationToken = default)
    {
        if (!oldEntities.IsNullOrEmpty())
            await DeleteAsync(oldEntities, cancellationToken);

        if (!newEntities.IsNullOrEmpty())
            await AddRangeAsync(newEntities, cancellationToken);
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = _dbSet.AsEnumerable();

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        await InternalSaveChangesAsync(cancellationToken);
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
    public virtual async Task ExecuteUpdateAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
        => await ExecuteUpdateAsync(i => i.Id == id, propertyBuilder, cancellationToken);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
    {
        if (!propertyBuilder.AuditCallsAdded)
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);
        }

        await _dbSet.Where(predicate).ExecuteUpdateAsync(propertyBuilder.SetPropertyCalls, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteDeleteAsync(object id, CancellationToken cancellationToken = default)
        => await ExecuteDeleteAsync(i => i.Id == id, cancellationToken: cancellationToken);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        switch (_dbContext.GetCurrentSoftDeletionState())
        {
            case SoftDeletionState.Active:

                var propertyBuilder = new SetPropertyBuilder<TEntity>();

                //Soft delete
                AddDeletionPropertyCalls(propertyBuilder);

                await _dbSet.Where(predicate).ExecuteUpdateAsync(propertyBuilder.SetPropertyCalls, cancellationToken: cancellationToken);

                break;
            case SoftDeletionState.Passive:

                await _dbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken: cancellationToken);

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Bulk add operation. 
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
    /// Bulk update operation. 
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
    /// Bulk delete operation. 
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
    public SetPropertyBuilder<TEntity> GetUpdatablePropertiesBuilder<TDto>(TDto dto) where TDto : DtoBase
        => _dbContext.GetUpdatablePropertiesBuilder<TEntity, TDto>(dto);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _dbContext.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) => await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken);

    #region Private Helper Methods

    /// <summary>
    /// Detach entity if found in local store.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isUpdate"></param>
    protected void DetachFromLocalIfExists(TEntity entity)
    {
        var localEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(u => u.GetUniqueIdentifier().Equals(entity.GetUniqueIdentifier()));

        if (localEntity != null)
        {
            var entry = _dbContext.Entry(localEntity);
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Save changes according to <see cref="_saveChangesAfterEveryOperation"/>.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task InternalSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_saveChangesAfterEveryOperation)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Save changes according to <see cref="_saveChangesAfterEveryOperation"/>.
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
    /// Creates Id == <paramref name="key"/> equality expression and append to <see cref="CommonHelper.CreateIsDeletedFalseExpression"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected Expression<Func<TEntity, bool>> CreateKeyEqualityExpression(object key, Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        Expression<Func<TEntity, bool>> idCondition = i => i.Id.Equals(key);

        var mainCondition = idCondition.Append(CreateConditionExpression(conditionExpression) ?? (entity => true), ExpressionType.AndAlso);

        return mainCondition.Append(conditionExpression, ExpressionType.AndAlso);
    }

    /// <summary>
    /// Creates property selector.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected static Expression<Func<TEntity, object>> CreateObjectPredicate(Type entityType, string propertyName)
    {
        CommonHelper.ThrowIfPropertyNotExists(entityType, propertyName);

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

        return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);
    }

    /// <summary>
    /// Checks pagination parameters are reasonable.
    /// </summary>
    /// <param name="totalDataCount"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="requestedPageNumber"></param>
    /// <returns></returns>
    protected static int CalculatePageCountAndCompareWithRequested(int totalDataCount, int countOfRequestedRecordsInPage, int requestedPageNumber)
    {
        var actualPageCount = Convert.ToDouble(totalDataCount) / Convert.ToDouble(countOfRequestedRecordsInPage);

        var estimatedCountOfPages = Convert.ToInt32(Math.Ceiling(actualPageCount));

        if (estimatedCountOfPages != 0 && requestedPageNumber > estimatedCountOfPages)
            throw new MilvaUserFriendlyException(MilvaException.WrongPaginationParams, estimatedCountOfPages);

        return estimatedCountOfPages;
    }

    /// <summary>
    /// Checks the parameters is valid.
    /// </summary>
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    protected static void ValidatePaginationParameters(int requestedPageNumber, int countOfRequestedRecordsInPage)
    {
        if (requestedPageNumber <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        if (countOfRequestedRecordsInPage <= 0)
            throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);
    }

    /// <summary>
    /// If <see cref="_softDeletedFetching"/> is false,  appends is deleted false expression to <paramref name="conditionExpression"/>.
    /// Else does nothing to <paramref name="conditionExpression"/> but if <see cref="_resetSoftDeletedFetchState"/> is true then sets <see cref="_softDeletedFetching"/> false.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected Expression<Func<TEntity, bool>> CreateConditionExpression(Expression<Func<TEntity, bool>> conditionExpression = null)
        => CreateConditionExpression<TEntity>(conditionExpression);

    /// <summary>
    /// If <see cref="_softDeletedFetching"/> is false,  appends is deleted false expression to <paramref name="conditionExpression"/>.
    /// Else does nothing to <paramref name="conditionExpression"/> but if <see cref="_resetSoftDeletedFetchState"/> is true then sets <see cref="_softDeletedFetching"/> false.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected Expression<Func<TResult, bool>> CreateConditionExpression<TResult>(Expression<Func<TResult, bool>> conditionExpression = null)
    {
        Expression<Func<TResult, bool>> mainExpression;

        //Step in when _softDeletedFetching is false
        if (!_softDeletedFetching)
        {
            var softDeleteExpression = CommonHelper.CreateIsDeletedFalseExpression<TResult>();

            mainExpression = softDeleteExpression.Append(conditionExpression, ExpressionType.AndAlso);
        }
        else
        {
            mainExpression = conditionExpression;

            if (_resetSoftDeletedFetchState)
                _softDeletedFetching = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;
        }

        return mainExpression;
    }

    /// <summary>
    /// Returns <see cref="QueryTrackingBehavior"/> according to <paramref name="tracking"/>.
    /// </summary>
    /// <param name="tracking"></param>
    /// <returns></returns>
    protected static QueryTrackingBehavior GetQueryTrackingBehavior(bool tracking) => tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTrackingWithIdentityResolution;

    #region Auditing

    /// <summary>
    /// Adds perform time, performer and IsDeleted proeperty calls to <paramref name="propertyBuilder"/>. 
    /// </summary>
    /// <param name="propertyBuilder"></param>
    protected internal virtual void AddDeletionPropertyCalls(SetPropertyBuilder<TEntity> propertyBuilder)
    {
        if (_dataAccessConfiguration.Auditing.AuditDeletionDate)
            AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.DeletionDate);

        if (CommonHelper.PropertyExists<TEntity>(EntityPropertyNames.IsDeleted))
        {
            var isDeletedPropertyExpression = CommonHelper.CreatePropertySelector<TEntity, bool>(EntityPropertyNames.IsDeleted);

            propertyBuilder.SetPropertyValue(isDeletedPropertyExpression, true);
        }

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

            var now = _dataAccessConfiguration.DbContext.UseUtcForDateTime ? DateTime.UtcNow : DateTime.Now;

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
            var currentUserName = _dataAccessConfiguration.DbContext.GetCurrentUserNameMethod.Invoke(_dbContext.ServiceProvider);

            if (!string.IsNullOrWhiteSpace(currentUserName))
            {
                var performerUserNamePropertyExpression = CommonHelper.CreatePropertySelector<TEntity, string>(propertyName);

                propertyBuilder.SetPropertyValue(performerUserNamePropertyExpression, currentUserName);
            }
        }
    }

    #endregion

    #endregion
}
