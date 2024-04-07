using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;
using Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
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

    #endregion

    #region Private Properties

    private readonly IDataAccessConfiguration _dataAccessConfiguration;
    private bool _resetSoftDeletedFetchState;
    private bool _softDeletedFetching;
    private bool _saveChangesAfterEveryOperation;

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
        _resetSoftDeletedFetchState = _dataAccessConfiguration.Repository.ResetSoftDeletedFetchStateToDefault;
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
    public void FetchSoftDeletedEntitiesInNextProcess(bool state = false) => _softDeletedFetching = state;

    /// <summary>
    /// Resets soft deleted entity fetch style to default.
    /// </summary>
    public void ResetSoftDeletedEntityFetchState() => _resetSoftDeletedFetchState = _dataAccessConfiguration.Repository.DefaultSoftDeletedFetchState;

    #endregion

    #region Async Data Access

    #region Async FirstOrDefault 

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                                       Expression<Func<TResult, bool>> conditionExpression = null,
                                                                       bool tracking = false,
                                                                       CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Select(projectionExpression)
                       .FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                                       Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                       bool tracking = false,
                                                                       CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression)
                       .FirstOrDefaultAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Select(projectionExpression ?? (entity => entity))
                       .FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns first entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetFirstOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    #endregion

    #region Async SingleOrDefault

    /// <summary>
    /// Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TResult> GetSingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                                        Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                        bool tracking = false,
                                                                        CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression)
                       .SingleOrDefaultAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false,
                                                               CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Select(projectionExpression ?? (entity => entity))
                       .SingleOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetSingleOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false,
                                                               CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .SingleOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    #endregion

    #region Async GetById

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    public virtual async Task<TResult> GetByIdAsync<TResult>(object id,
                                                             Expression<Func<TEntity, TResult>> projectionExpression,
                                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                                             bool tracking = false,
                                                             CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .Where(mainCondition)
                           .Select(projectionExpression)
                           .SingleOrDefaultAsync(cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    public virtual async Task<TEntity> GetByIdAsync(object id,
                                                    Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                    Expression<Func<TEntity, bool>> conditionExpression = null,
                                                    bool tracking = false,
                                                    CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .Select(projectionExpression ?? (entity => entity))
                           .SingleOrDefaultAsync(mainCondition, cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <summary>
    ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    public virtual async Task<TEntity> GetByIdAsync(object id,
                                                    Func<IIncludable<TEntity>, IIncludable> includes,
                                                    Expression<Func<TEntity, bool>> conditionExpression = null,
                                                    Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                    bool tracking = false,
                                                    CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                           .IncludeMultiple(includes)
                           .Select(projectionExpression ?? (entity => entity))
                           .SingleOrDefaultAsync(mainCondition, cancellationToken)
                           .ConfigureAwait(false);
    }

    #endregion

    #region Async GetAll

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="listRequest"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ListResponse<TResult>> GetAllAsync<TResult>(ListRequest listRequest,
                                                                          Expression<Func<TEntity, TResult>> projectionExpression,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                          bool tracking = false,
                                                                          CancellationToken cancellationToken = default) where TResult : class
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression)
                       .ToListResponseAsync(listRequest, cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> projectionExpression,
                                                                         Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                         bool tracking = false,
                                                                         CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression)
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Returns entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ListResponse<TEntity>> GetAllAsync(ListRequest listRequest,
                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListResponseAsync(listRequest, cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                bool tracking = false,
                                                                CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                bool tracking = false,
                                                                CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                         Expression<Func<TEntity, TResult>> projectionExpression,
                                                                         Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                         bool tracking = false,
                                                                         CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression)
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    #endregion

    #region Async GetSome

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TResult>> GetSomeAsync<TResult>(int count,
                                                                          Expression<Func<TEntity, TResult>> projectionExpression,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                          bool tracking = false,
                                                                          CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Take(count)
                       .Select(projectionExpression)
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetSomeAsync(int count,
                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Take(count)
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetSomeAsync(int count,
                                                                 Func<IIncludable<TEntity>, IIncludable> includes,
                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = default)
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Take(count)
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

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

        await SaveChangesAsync(cancellationToken);
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

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity,
                                          CancellationToken cancellationToken = default,
                                          params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        var dbEntry = _dbContext.Entry(entity);

        foreach (var includeProperty in projectionProperties)
            dbEntry.Property(includeProperty).IsModified = true;

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(IEnumerable<TEntity> entities,
                                          CancellationToken cancellationToken = default,
                                          params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        if (entities.IsNullOrEmpty())
            return;

        foreach (var entity in entities)
        {
            var dbEntry = _dbContext.Entry(entity);

            foreach (var includeProperty in projectionProperties)
                dbEntry.Property(includeProperty).IsModified = true;
        }

        await SaveChangesAsync(cancellationToken);
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

        _dbSet.UpdateRange(entities);

        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);

        await SaveChangesAsync(cancellationToken);
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

        _dbSet.RemoveRange(entities);

        await SaveChangesAsync(cancellationToken);
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
            _dbSet.RemoveRange(oldEntities);

        if (!newEntities.IsNullOrEmpty())
            _dbSet.AddRange(newEntities);

        await SaveChangesAsync(cancellationToken);
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
            await DeleteAsync(oldEntities, cancellationToken).ConfigureAwait(false);

        if (!newEntities.IsNullOrEmpty())
            await AddRangeAsync(newEntities, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = _dbSet.AsEnumerable();

        _dbSet.RemoveRange(entities);

        await SaveChangesAsync(cancellationToken);
    }

    #region Bulk Async

    /// <summary>
    /// Runs execute update. Adds performer and perform time to to be updated properties.
    /// You can detect non null properties and create <see cref="SetPropertyBuilder{TSource}"/> with <see cref="MilvaEfExtensions.GetSetPropertyBuilderFromDto"/> method.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteUpdateAsync(object id, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
        => await ExecuteUpdateAsync(i => i.Id == id, propertyBuilder, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Runs execute update with given <paramref name="predicate"/>. Adds performer and perform time to to be updated properties.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="propertyBuilder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate, SetPropertyBuilder<TEntity> propertyBuilder, CancellationToken cancellationToken = default)
    {
        if (_dataAccessConfiguration.Auditing.AuditModificationDate)
            AddPerformTimePropertyCall(propertyBuilder, EntityPropertyNames.LastModificationDate);

        if (_dataAccessConfiguration.Auditing.AuditModifier)
            AddPerformerUserPropertyCall(propertyBuilder, EntityPropertyNames.LastModifierUserName);

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

        await SaveChangesBulkAsync(bulkConfig, cancellationToken);
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

        await SaveChangesBulkAsync(bulkConfig, cancellationToken);
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

        await SaveChangesBulkAsync(bulkConfig, cancellationToken);
    }

    #endregion

    #endregion

    #endregion

    #region Private Helper Methods

    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_saveChangesAfterEveryOperation)
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SaveChangesBulkAsync(BulkConfig bulkConfig = null, CancellationToken cancellationToken = default)
    {
        if (_saveChangesAfterEveryOperation)
            await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates Id == <paramref name="key"/> equality expression and append to <see cref="CommonHelper.CreateIsDeletedFalseExpression"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected static Expression<Func<TEntity, bool>> CreateKeyEqualityExpression(object key, Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        Expression<Func<TEntity, bool>> idCondition = i => i.Id.Equals(key);

        var mainCondition = idCondition.Append(CommonHelper.CreateIsDeletedFalseExpression<TEntity>(), ExpressionType.AndAlso);

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
            var performerUserNameExpression = CommonHelper.CreatePropertySelector<TEntity, bool>(EntityPropertyNames.IsDeleted);

            propertyBuilder.SetPropertyValue(performerUserNameExpression, true);
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
            var performerUserNameExpression = CommonHelper.CreatePropertySelector<TEntity, DateTime>(propertyName);

            var now = _dataAccessConfiguration.DbContext.UseUtcForDateTimes ? DateTime.UtcNow : DateTime.Now;

            propertyBuilder.SetPropertyValue(performerUserNameExpression, now);
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
                var performerUserNameExpression = CommonHelper.CreatePropertySelector<TEntity, string>(propertyName);

                propertyBuilder.SetPropertyValue(performerUserNameExpression, currentUserName);
            }
        }
    }

    #endregion

    #endregion
}
