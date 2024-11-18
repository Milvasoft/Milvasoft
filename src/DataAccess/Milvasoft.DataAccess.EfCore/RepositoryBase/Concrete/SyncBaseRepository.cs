using Microsoft.EntityFrameworkCore;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories.
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
                 .Select(UpdateProjectionExpression(projection))
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
                 .Select(UpdateProjectionExpression(projection))
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
    /// <param name="projection"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TResult GetById<TResult>(object id,
                                            Expression<Func<TEntity, bool>> condition = null,
                                            Expression<Func<TEntity, TResult>> projection = null,
                                            Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                            bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Where(mainCondition)
                     .Select(UpdateProjectionExpression(projection))
                     .SingleOrDefault(conditionAfterProjection ?? (entity => true));
    }

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
    public virtual TEntity GetForDelete(object id,
                                        Func<IIncludable<TEntity>, IIncludable> includes = null,
                                        Expression<Func<TEntity, bool>> condition = null,
                                        bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        if (includes is not null)
            return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                         .Where(mainCondition)
                         .IncludeMultiple(includes)
                         .SingleOrDefault();

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(mainCondition);

        return IncludeNavigationProperties(query).SingleOrDefault();
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
    /// <returns> The entity found or null. </returns>
    public virtual TResult GetForDelete<TResult>(object id,
                                                 Func<IIncludable<TEntity>, IIncludable> includes = null,
                                                 Expression<Func<TEntity, bool>> condition = null,
                                                 Expression<Func<TEntity, TResult>> projection = null,
                                                 Expression<Func<TResult, bool>> conditionAfterProjection = null,
                                                 bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpressionWithIsDeletedFalse(id, condition);

        if (includes is not null)
            return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                         .Where(mainCondition)
                         .IncludeMultiple(includes)
                         .Select(UpdateProjectionExpression(projection))
                         .SingleOrDefault(conditionAfterProjection ?? (entity => true));

        var query = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking)).Where(mainCondition);

        return IncludeNavigationProperties(query).Select(UpdateProjectionExpression(projection))
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
                 .Select(UpdateProjectionExpression(projection))
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
                 .Select(UpdateProjectionExpression(projection))
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
                 .Select(UpdateProjectionExpression(projection))
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
    public virtual int Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        return InternalSaveChanges();
    }

    /// <summary>
    ///  Adds multiple entities to database synchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int AddRange(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        _dbSet.AddRange(entities);

        return InternalSaveChanges();
    }

    /// <summary>
    /// Updates specified entity in database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual int Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Update(entity);

        return InternalSaveChanges();
    }

    /// <summary>
    /// Updates multiple entities in database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Update(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Update(entity);
        }

        return InternalSaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual int Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (propertySelectors.IsNullOrEmpty())
            return 0;

        DetachFromLocalIfExists(entity);

        var entry = _dbSet.Entry(entity);

        foreach (var includeProperty in propertySelectors)
            entry.Property(includeProperty).IsModified = true;

        return InternalSaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    public virtual int Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] propertySelectors)
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

        return InternalSaveChanges();
    }

    /// <summary>
    ///  Deletes single entity from database synchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual int Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachFromLocalIfExists(entity);
        _dbSet.Remove(entity);

        return InternalSaveChanges();
    }

    /// <summary>
    ///  Deletes multiple entity from database synchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual int Delete(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return 0;

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        return InternalSaveChanges();
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual int ReplaceOldsWithNews(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        if (!oldEntities.IsNullOrEmpty())
            foreach (var entity in oldEntities)
            {
                DetachFromLocalIfExists(entity);
                _dbSet.Remove(entity);
            }

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);

        return InternalSaveChanges();
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual int ReplaceOldsWithNewsInSeperateDatabaseProcess(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        int affectedRows = 0;

        if (!oldEntities.IsNullOrEmpty())
            affectedRows += Delete(oldEntities);

        if (!newEntities.IsNullOrEmpty())
            affectedRows += AddRange(newEntities);

        return affectedRows;
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual int RemoveAll()
    {
        var entities = _dbSet.AsEnumerable();

        foreach (var entity in entities)
        {
            DetachFromLocalIfExists(entity);
            _dbSet.Remove(entity);
        }

        return InternalSaveChanges();
    }

    #region Bulk 

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetUpdatablePropertiesBuilder"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public virtual int ExecuteUpdate(object id, SetPropertyBuilder<TEntity> propertyBuilder)
        => ExecuteUpdate(i => i.Id == id, propertyBuilder);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public virtual int ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder)
    {
        if (!propertyBuilder.AuditCallsAdded)
        {
            if (_dataAccessConfiguration.Auditing.AuditModificationDate)
                AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

            if (_dataAccessConfiguration.Auditing.AuditModifier)
                AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);
        }

        return _dbSet.Where(predicate).ExecuteUpdate(propertyBuilder.SetPropertyCalls);
    }

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <returns></returns>
    public virtual int ExecuteDelete(object id, SetPropertyBuilder<TEntity> propertyBuilder = null)
        => ExecuteDelete(i => i.Id == id, propertyBuilder);

    /// <summary>
    /// Deletes all records that given <paramref name="predicate"/>. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"> If soft delete is active you may want to update some properties. etc. image in database</param>
    /// <returns></returns>
    public virtual int ExecuteDelete(Expression<Func<TEntity, bool>> predicate,
                                     SetPropertyBuilder<TEntity> propertyBuilder = null)
    {
        // If soft deletion active and entity is soft deletable.
        if (_dbContext.GetCurrentSoftDeletionState() == SoftDeletionState.Active && CommonHelper.PropertyExists<TEntity>(EntityPropertyNames.IsDeleted))
        {
            propertyBuilder ??= new SetPropertyBuilder<TEntity>();

            //Soft delete
            AddDeletionPropertyCalls(propertyBuilder);

            return _dbSet.Where(predicate).ExecuteUpdate(propertyBuilder.SetPropertyCalls);
        }

        return _dbSet.Where(predicate).ExecuteDelete();
    }

    #endregion

    #endregion

    #endregion

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    public int SaveChanges() => _dbContext.SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <returns></returns>
    protected int InternalSaveChanges()
    {
        if (_saveChangesAfterEveryOperation)
            return _dbContext.SaveChanges();

        return 0;
    }
}
