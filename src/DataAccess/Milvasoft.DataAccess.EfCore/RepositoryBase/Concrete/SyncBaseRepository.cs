using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
public abstract partial class BaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaDbContextBase
{
    #region Data Access

    #region FirstOrDefault 

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> condition = null, bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .FirstOrDefault(CreateConditionExpression(condition) ?? (entity => true));

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                      Expression<Func<TEntity, TResult>> projection = null,
                                                      Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                      bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(condition) ?? (entity => true))
                 .Select(projection)
                 .FirstOrDefault(conditionAfterProjection ?? (entity => true));

    #endregion

    #region  SingleOrDefault

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> condition = null, bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .SingleOrDefault(CreateConditionExpression(condition) ?? (entity => true));

    /// <summary>
    /// Returns single entity or default value which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TResult GetSingleOrDefault<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                       Expression<Func<TEntity, TResult>> projection = null,
                                                       Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                       bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(condition) ?? (entity => true))
                 .Select(projection)
                 .SingleOrDefault(conditionAfterProjection ?? (entity => true));

    #endregion

    #region  GetById

    /// <summary>
    /// Returns one entity by entity Id from database synchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TEntity GetById(object id,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .SingleOrDefault(mainCondition);
    }

    /// <summary>
    /// Returns one entity by entity Id from database synchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TResult GetById<TResult>(object id,
                                            Expression<Func<TEntity, bool>> condition = null,
                                            Expression<Func<TEntity, TResult>> projectionExpression = null,
                                            Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                            bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Where(mainCondition)
                     .Select(projectionExpression)
                     .SingleOrDefault(conditionAfterProjection ?? (entity => true));
    }

    #endregion

    #region  GetAll

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual ListResponse<TEntity> GetAll(ListRequest listRequest,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .ToListResponse(listRequest);

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
    public virtual ListResponse<TResult> GetAll<TResult>(ListRequest listRequest,
                                                         Expression<Func<TEntity, bool>> condition = null,
                                                         Expression<Func<TEntity, TResult>> projection = null,
                                                         Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                         bool tracking = false) where TResult : class
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(condition) ?? (entity => true))
                 .Select(projection)
                 .Where(conditionAfterProjection ?? (entity => true))
                 .ToListResponse(listRequest);

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual List<TEntity> GetAll(Expression<Func<TEntity, bool>> conditionExpression = null, bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .ToList();

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="condition"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual List<TResult> GetAll<TResult>(Expression<Func<TEntity, bool>> condition = null,
                                                 Expression<Func<TEntity, TResult>> projection = null,
                                                 Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                 bool tracking = false) where TResult : class
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(condition) ?? (entity => true))
                 .Select(projection)
                 .Where(conditionAfterProjection ?? (entity => true))
                 .ToList();

    #endregion

    #region  GetSome

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual List<TEntity> GetSome(int count,
                                         Expression<Func<TEntity, bool>> conditionExpression = null,
                                         bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .ToList();

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="condition"></param>
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionAfterProjection"></param>
    /// <returns></returns>
    public virtual List<TResult> GetSome<TResult>(int count,
                                                  Expression<Func<TEntity, bool>> condition = null,
                                                  Expression<Func<TEntity, TResult>> projection = null,
                                                  Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                  bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(condition) ?? (entity => true))
                 .Select(projection)
                 .Where(conditionAfterProjection ?? (entity => true))
                 .Take(count)
                 .ToList();

    #endregion

    #region  Insert/Update/Delete

    /// <summary>
    ///  Adds single entity to database synchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        InternalSaveChanges();
    }

    /// <summary>
    ///  Adds multiple entities to database synchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        _dbSet.AddRange(entities);

        InternalSaveChanges();
    }

    /// <summary>
    /// Updates specified entity in database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Update(entity);

        InternalSaveChanges();
    }

    /// <summary>
    /// Updates multiple entities in database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="()"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Update(entity);
        }

        InternalSaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual void Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (propertySelectors.IsNullOrEmpty())
            return;

        DetachFromLocalIfExists(entity);

        var entry = _dbSet.Entry(entity);

        foreach (var includeProperty in propertySelectors)
            entry.Property(includeProperty).IsModified = true;

        InternalSaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] propertySelectors)
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

        InternalSaveChanges();
    }

    /// <summary>
    ///  Deletes single entity from database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Remove(entity);

        InternalSaveChanges();
    }

    /// <summary>
    ///  Deletes multiple entity from database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void Delete(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        InternalSaveChanges();
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual void ReplaceOldsWithNews(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        if (!oldEntities.IsNullOrEmpty())
            foreach (var entity in oldEntities)
            {
                DetachFromLocalIfExists(entity);
                _dbSet.Remove(entity);
            }

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);

        InternalSaveChanges();
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual void ReplaceOldsWithNewsInSeperateDatabaseProcess(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        if (!oldEntities.IsNullOrEmpty())
            Delete(oldEntities);

        if (!newEntities.IsNullOrEmpty())
            AddRange(newEntities);
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual void RemoveAll()
    {
        var entities = _dbSet.AsEnumerable();

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        InternalSaveChanges();
    }

    #region Bulk 

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public virtual void ExecuteUpdate(object id, SetPropertyBuilder<TEntity> propertyBuilder)
        => ExecuteUpdate(i => i.Id == id, propertyBuilder);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public virtual void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder)
    {
        if (!propertyBuilder.AuditCallsAdded)
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);
        }

        _dbSet.Where(predicate).ExecuteUpdate(propertyBuilder.SetPropertyCalls);
    }

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual void ExecuteDelete(object id)
        => ExecuteDelete(i => i.Id == id);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual void ExecuteDelete(Expression<Func<TEntity, bool>> predicate)
    {
        switch (_dbContext.GetCurrentSoftDeletionState())
        {
            case SoftDeletionState.Active:

                var propertyBuilder = new SetPropertyBuilder<TEntity>();

                //Soft delete
                AddDeletionPropertyCalls(propertyBuilder);

                _dbSet.Where(predicate).ExecuteUpdate(propertyBuilder.SetPropertyCalls);

                break;
            case SoftDeletionState.Passive:

                _dbSet.Where(predicate).ExecuteDelete();

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

    #endregion

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    public void SaveChanges() => _dbContext.SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public void SaveChangesBulk(BulkConfig bulkConfig = null) => _dbContext.SaveChangesBulk(bulkConfig);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    protected void InternalSaveChanges()
    {
        if (_saveChangesAfterEveryOperation)
            _dbContext.SaveChanges();
    }

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
