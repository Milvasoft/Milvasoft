using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Concrete;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
using System.Linq.Expressions;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Components.Rest.Response;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
public abstract partial class BaseRepository<TEntity, TContext> where TEntity : class, IMilvaEntity where TContext : DbContext, IMilvaDbContextBase
{
    #region Sync Data Access

    #region Sync FirstOrDefault 

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                      bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression)
                 .FirstOrDefault();

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Select(projectionExpression ?? (entity => entity))
                 .FirstOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    /// <summary>
    ///  Returns first entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetFirstOrDefault(Func<IIncludable<TEntity>, IIncludable> includes,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .FirstOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    #endregion

    #region Sync SingleOrDefault

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TResult GetSingleOrDefault<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                       Expression<Func<TEntity, bool>> conditionExpression = null,
                                                       bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression)
                 .SingleOrDefault();

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                              bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Select(projectionExpression ?? (entity => entity))
                 .SingleOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TEntity GetSingleOrDefault(Func<IIncludable<TEntity>, IIncludable> includes,
                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                              bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .SingleOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    #endregion

    #region Sync GetById

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TResult GetById<TResult>(object id,
                                            Expression<Func<TEntity, TResult>> projectionExpression,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Where(mainCondition)
                     .Select(projectionExpression)
                     .SingleOrDefault();
    }

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TEntity GetById(object id,
                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Select(projectionExpression ?? (entity => entity))
                     .SingleOrDefault(mainCondition);
    }

    /// <summary>
    ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TEntity GetById(object id,
                                   Func<IIncludable<TEntity>, IIncludable> includes,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                   bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .IncludeMultiple(includes)
                     .Select(projectionExpression ?? (entity => entity))
                     .SingleOrDefault(mainCondition);
    }

    #endregion

    #region Sync GetAll

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="listRequest"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual ListResponse<TResult> GetAll<TResult>(ListRequest listRequest,
                                                         Expression<Func<TEntity, TResult>> projectionExpression,
                                                         Expression<Func<TEntity, bool>> conditionExpression = null,
                                                         bool tracking = false) where TResult : class
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression)
                 .ToListResponse(listRequest);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual IEnumerable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                        Expression<Func<TEntity, bool>> conditionExpression = null,
                                                        bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression)
                 .ToList();

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual ListResponse<TEntity> GetAll(ListRequest listRequest,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression ?? (entity => entity))
                 .ToListResponse(listRequest);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                               bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAll(Func<IIncludable<TEntity>, IIncludable> includes,
                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                               bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    /// <summary>
    /// Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual IEnumerable<TResult> GetAll<TResult>(Func<IIncludable<TEntity>, IIncludable> includes,
                                                        Expression<Func<TEntity, TResult>> projectionExpression,
                                                        Expression<Func<TEntity, bool>> conditionExpression = null,
                                                        bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression)
                 .ToList();

    #endregion

    #region Sync GetSome

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TResult> GetSome<TResult>(int count,
                                                         Expression<Func<TEntity, TResult>> projectionExpression,
                                                         Expression<Func<TEntity, bool>> conditionExpression = null,
                                                         bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .Select(projectionExpression)
                 .ToList();

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetSome(int count,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetSome(int count,
                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    #endregion

    #region Sync Insert/Update/Delete

    /// <summary>
    ///  Adds single entity to database asynchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        SaveChanges();
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        _dbSet.AddRange(entities);

        SaveChanges();
    }

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);

        SaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual void Update(TEntity entity, params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        var dbEntry = _dbContext.Entry(entity);

        foreach (var includeProperty in projectionProperties)
            dbEntry.Property(includeProperty).IsModified = true;

        SaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            var dbEntry = _dbContext.Entry(entity);

            foreach (var includeProperty in projectionProperties)
                dbEntry.Property(includeProperty).IsModified = true;
        }

        SaveChanges();
    }

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        _dbSet.UpdateRange(entities);

        SaveChanges();
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);

        SaveChanges();
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void Delete(IEnumerable<TEntity> entities)
    {
        if (entities.IsNullOrEmpty())
            return;

        _dbSet.RemoveRange(entities);

        SaveChanges();
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
            _dbSet.RemoveRange(oldEntities);

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);

        SaveChanges();
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
            _dbSet.RemoveRange(oldEntities);

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual void RemoveAll()
    {
        var entities = _dbSet.AsEnumerable();

        _dbSet.RemoveRange(entities);

        SaveChanges();
    }

    #region Bulk Async

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetSetPropertyBuilderFromDto"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>

    /// <returns></returns>
    public virtual void ExecuteUpdate(object id, SetPropertyBuilder<TEntity> propertyBuilder)        => ExecuteUpdate(i => i.Id == id, propertyBuilder);

    /// <summary>
    /// Deletes all records that match the condition. If <see cref="SoftDeletionState"/> is active, it updates the soft delete properties of the relevant entity. 
    /// Note that this will not work with navigation properties.
    /// </summary>
    /// <param name="id"></param> 
    /// <returns></returns>
    public virtual void ExecuteDelete(object id)
        => ExecuteDelete(i => i.Id == id);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <returns></returns>
    public virtual void ExecuteUpdate(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder)
    {
        if (_dataAccessConfiguration.Auditing.AuditModificationDate)
            AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

        if (_dataAccessConfiguration.Auditing.AuditModifier)
            AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);

        _dbSet.Where(predicate).ExecuteUpdate(propertyBuilder.SetPropertyCalls);
    }

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

        SaveChangesBulk(bulkConfig);
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

        SaveChangesBulk(bulkConfig);
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

        SaveChangesBulk(bulkConfig);
    }

    #endregion

    #endregion

    #endregion

    private void SaveChanges()
    {
        if (_saveChangesAfterEveryOperation.Value)
            _dbContext.SaveChanges();
    }

    private void SaveChangesBulk(BulkConfig bulkConfig = null)
    {
        if (_saveChangesAfterEveryOperation.Value)
            _dbContext.SaveChangesBulk(bulkConfig);
    }
}
