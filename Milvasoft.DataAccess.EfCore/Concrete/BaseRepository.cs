﻿using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core;
using Milvasoft.Core.EntityBase.Abstract;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.DataAccess.EfCore.Abstract;
using Milvasoft.DataAccess.EfCore.Concrete;
using Milvasoft.DataAccess.EfCore.IncludeLibrary;
using Milvasoft.DataAccess.EfCore.MilvaContext;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract partial class BaseRepository<TEntity, TKey, TContext> : IBaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                                                                                 where TKey : struct, IEquatable<TKey>
                                                                                                                 where TContext : DbContext, IMilvaDbContextBase
{
    //TODO EntityFrameworkQueryableExtensions methods will be added here.
    //TODO Sync methods will be added.

    #region Public Properties

    /// <summary>
    /// All repository methods save changes(<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>) after every transaction. Except get operations. 
    /// If you use unit of work pattern or save changes manually you can set to false this variable.
    /// </summary>
    public static bool SaveChangesAfterEveryTransaction { get; set; } = true;

    /// <summary>
    /// Set to true when using no tracking.
    /// </summary>
    public static bool InitializeUpdate { get; set; } = true;

    #endregion

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

    private static bool _resetGetSoftDeletedEntitiesState = true;
    private static bool _getSoftDeletedEntities = false;

    #endregion

    /// <summary>
    /// Constructor of BaseRepository for <paramref name="dbContext"/> injection.
    /// </summary>
    /// <param name="dbContext"></param>
    public BaseRepository(TContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    /// <summary>
    /// Determines whether save changes method called after evert repository method.
    /// <para><b>Default is true.</b></para>
    /// </summary>
    /// <param name="state"></param>
    public void ChangeSaveChangesAfterEveryTransactionState(bool state) => SaveChangesAfterEveryTransaction = state;

    /// <summary>
    /// Determines whether applied detach to updating entites. 
    /// <para><b>Default is true.</b></para>
    /// </summary>
    /// <param name="state"></param>
    public void ChangeInitializeUpdateState(bool state) => InitializeUpdate = state;

    /// <summary>
    /// Determines whether soft deleted entities in the database are fetched from the database.
    /// <para><b>Default is false.</b></para>
    /// </summary>
    /// <param name="state"></param>
    public void GetSoftDeletedEntitiesInNextProcess(bool state) => _getSoftDeletedEntities = state;

    /// <summary>
    /// Determines whether the default value of the variable that determines the status of deleted data in the database is assigned to the default value after database operation.
    /// </summary>
    /// <param name="state"></param>
    public void ResetSoftDeleteState(bool state) => _resetGetSoftDeletedEntitiesState = state;

    #region Get Data  

    /// <summary>
    ///  Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = new CancellationToken())
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
                                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                               bool tracking = false,
                                                               CancellationToken cancellationToken = new CancellationToken())
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
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                               bool tracking = false,
                                                               CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .SingleOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true), cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                bool tracking = false,
                                                                CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
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
                                                                         CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression)
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
                                                                CancellationToken cancellationToken = new CancellationToken())
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
                                                                         CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .IncludeMultiple(includes)
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
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = new CancellationToken())
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
                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                 bool tracking = false,
                                                                 CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Take(count)
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    #region Pagination And Order

    /// <summary>
    ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range. 
    ///       If the condition is requested, it also provides that condition.
    /// </summary>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    /// 
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                                      int countOfRequestedRecordsInPage,
                                                                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                      bool tracking = false,
                                                                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        var repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(condition ?? (entity => true))
                               .Select(projectionExpression ?? (entity => entity))
                               .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                               .Take(countOfRequestedRecordsInPage)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
    ///       If the condition is requested, it also provides that condition.
    /// </summary>
    ///
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    ///
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                                      int countOfRequestedRecordsInPage,
                                                                                                                      Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                      bool tracking = false,
                                                                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        var repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                               .Where(condition ?? (entity => true))
                               .IncludeMultiple(includes)
                               .Select(projectionExpression ?? (entity => entity))
                               .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                               .Take(countOfRequestedRecordsInPage)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates and ordered asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
    ///       If the condition is requested, it also provides that condition.
    ///       
    /// </summary>
    /// 
    /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    /// 
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                                string orderByPropertyName,
                                                                                                                                bool orderByAscending,
                                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                                bool tracking = false,
                                                                                                                                CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                 .Where(condition ?? (entity => true))
                                                 .Select(projectionExpression ?? (entity => entity))
                                                 .OrderBy(predicate)
                                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                 .Take(countOfRequestedRecordsInPage)
                                                 .ToListAsync(cancellationToken)
                                                 .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                .Where(condition ?? (entity => true))
                                .Select(projectionExpression ?? (entity => entity))
                                .OrderByDescending(predicate)
                                .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                .Take(countOfRequestedRecordsInPage)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates and ordered asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
    ///        If the condition is requested, it also provides that condition.
    ///        
    /// </summary>
    /// 
    /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    ///
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="includes"></param>
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                                string orderByPropertyName,
                                                                                                                                bool orderByAscending,
                                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                                bool tracking = false,
                                                                                                                                CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                  .Where(condition ?? (entity => true))
                                                  .OrderBy(predicate)
                                                  .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                  .Take(countOfRequestedRecordsInPage)
                                                  .IncludeMultiple(includes)
                                                  .Select(projectionExpression ?? (entity => entity))
                                                  .ToListAsync(cancellationToken)
                                                  .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                 .Where(condition ?? (entity => true))
                                 .OrderByDescending(predicate)
                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                 .Take(countOfRequestedRecordsInPage)
                                 .IncludeMultiple(includes)
                                 .Select(projectionExpression ?? (entity => entity))
                                 .ToListAsync(cancellationToken)
                                 .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
    ///       If the condition is requested, it also provides that condition.
    ///       
    /// </summary>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    /// 
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="orderByKeySelector"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                                Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                                bool orderByAscending,
                                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                                bool tracking = false,
                                                                                                                                CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                 .Where(condition ?? (entity => true))
                                                 .Select(projectionExpression ?? (entity => entity))
                                                 .OrderBy(orderByKeySelector)
                                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                 .Take(countOfRequestedRecordsInPage)
                                                 .ToListAsync(cancellationToken)
                                                 .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                .Where(condition ?? (entity => true))
                                .Select(projectionExpression ?? (entity => entity))
                                .OrderByDescending(orderByKeySelector)
                                .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                .Take(countOfRequestedRecordsInPage)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
    ///        If the condition is requested, it also provides that condition.
    ///        
    /// </summary>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
    ///
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="includes"></param>
    /// <param name="orderByKeySelector"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                                Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                                bool orderByAscending,
                                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                                bool tracking = false,
                                                                                                                                CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression, cancellationToken: cancellationToken).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                  .Where(condition ?? (entity => true))
                                                  .OrderBy(orderByKeySelector)
                                                  .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                  .Take(countOfRequestedRecordsInPage)
                                                  .IncludeMultiple(includes)
                                                  .Select(projectionExpression ?? (entity => entity))
                                                  .ToListAsync(cancellationToken)
                                                  .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                 .Where(condition ?? (entity => true))
                                 .OrderByDescending(orderByKeySelector)
                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                 .Take(countOfRequestedRecordsInPage)
                                 .IncludeMultiple(includes)
                                 .Select(projectionExpression ?? (entity => entity))
                                 .ToListAsync(cancellationToken)
                                 .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Gets entities as ordered with <paramref name="orderByPropertyName"/>.
    ///       If the condition is requested, it also provides that condition.
    ///       
    /// </summary>
    /// 
    /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
    /// 
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(string orderByPropertyName,
                                                                      bool orderByAscending,
                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                      bool tracking = false,
                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                 .Where(condition ?? (entity => true))
                                                 .Select(projectionExpression ?? (entity => entity))
                                                 .OrderBy(predicate)
                                                 .ToListAsync(cancellationToken)
                                                 .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                .Where(condition ?? (entity => true))
                                .Select(projectionExpression ?? (entity => entity))
                                .OrderByDescending(predicate)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);

        return repo;
    }

    /// <summary>
    ///  Gets entities with includes as ordered with <paramref name="orderByPropertyName"/>.
    ///        If the condition is requested, it also provides that condition.
    ///        
    /// </summary>
    ///
    /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
    /// 
    /// <param name="includes"></param>
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                      string orderByPropertyName,
                                                                      bool orderByAscending,
                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                      bool tracking = false,
                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                  .Where(condition ?? (entity => true))
                                                  .OrderBy(predicate)
                                                  .IncludeMultiple(includes)
                                                  .Select(projectionExpression ?? (entity => entity))
                                                  .ToListAsync(cancellationToken)
                                                  .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                 .Where(condition ?? (entity => true))
                                 .OrderByDescending(predicate)
                                 .IncludeMultiple(includes)
                                 .Select(projectionExpression ?? (entity => entity))
                                 .ToListAsync(cancellationToken)
                                 .ConfigureAwait(false);

        return repo;
    }

    /// <summary>
    ///  Gets entities as ordered with <paramref name="orderByKeySelector"/>.
    ///       If the condition is requested, it also provides that condition.
    ///       
    /// </summary>
    /// 
    /// <param name="orderByKeySelector"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Expression<Func<TEntity, object>> orderByKeySelector,
                                                                      bool orderByAscending,
                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                      bool tracking = false,
                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                 .Where(condition ?? (entity => true))
                                                 .Select(projectionExpression ?? (entity => entity))
                                                 .OrderBy(orderByKeySelector)
                                                 .ToListAsync(cancellationToken)
                                                 .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                .Where(condition ?? (entity => true))
                                .Select(projectionExpression ?? (entity => entity))
                                .OrderByDescending(orderByKeySelector)
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);

        return repo;
    }

    /// <summary>
    ///  Gets entities as ordered with <paramref name="orderByKeySelector"/>.
    ///        If the condition is requested, it also provides that condition.
    ///        
    /// </summary>
    ///
    /// <param name="includes"></param>
    /// <param name="orderByKeySelector"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                      Expression<Func<TEntity, object>> orderByKeySelector,
                                                                      bool orderByAscending,
                                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                      Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                      bool tracking = false,
                                                                      CancellationToken cancellationToken = new CancellationToken())
    {
        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                  .Where(condition ?? (entity => true))
                                                  .OrderBy(orderByKeySelector)
                                                  .IncludeMultiple(includes)
                                                  .Select(projectionExpression ?? (entity => entity))
                                                  .ToListAsync(cancellationToken)
                                                  .ConfigureAwait(false);
        else repo = await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                 .Where(condition ?? (entity => true))
                                 .OrderByDescending(orderByKeySelector)
                                 .IncludeMultiple(includes)
                                 .Select(projectionExpression ?? (entity => entity))
                                 .ToListAsync(cancellationToken)
                                 .ConfigureAwait(false);

        return repo;
    }

    #endregion

    /// <summary>
    /// Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity found or null. </returns>
    public virtual async Task<TEntity> GetByIdAsync(TKey id,
                                                    Expression<Func<TEntity, bool>> conditionExpression = null,
                                                    Expression<Func<TEntity, TEntity>> projectionExpression = null,
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
    ///  Returns one entity by entity Id from database asynchronously.
    /// </summary>
    /// 
    /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
    /// 
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity. </returns>
    public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id,
                                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                                            Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                            bool tracking = false,
                                                            CancellationToken cancellationToken = new CancellationToken())
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                            .Select(projectionExpression ?? (entity => entity))
                            .SingleAsync(mainCondition, cancellationToken)
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
    public virtual async Task<TEntity> GetByIdAsync(TKey id,
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


    /// <summary>
    ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// 
    /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
    /// 
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> The entity. </returns>
    public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id,
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
                           .SingleAsync(mainCondition, cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <summary>
    /// Groups entities with <paramref name="groupByPropertyName"/> and returns the key grouped and the number of items grouped with this key.
    /// </summary>
    /// <param name="groupByPropertyName"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(string groupByPropertyName,
                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                bool tracking = false,
                                                                                CancellationToken cancellationToken = new CancellationToken())
    {
        var entityType = typeof(TEntity);

        CheckProperty(groupByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, groupByPropertyName);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                            .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                            .GroupBy(predicate)
                            .Select(b => Tuple.Create(b.Key, b.Count()))
                            .ToListAsync(cancellationToken)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Groups entities with <paramref name="keySelector"/> and returns the key grouped and the number of items grouped with this key.
    /// </summary>
    /// <param name="keySelector"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(Expression<Func<TEntity, object>> keySelector,
                                                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                bool tracking = false,
                                                                                CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .GroupBy(keySelector)
                       .Select(b => Tuple.Create(b.Key, b.Count()))
                       .ToListAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Gets grouped entities from database with <paramref name="groupedClause"/>.
    /// 
    /// <para> <b>Example use;</b></para>
    /// <code>
    /// 
    /// <para>   var dbSet = _contextRepository.GetDbSet{Poco}();  <see cref="ContextRepository{TContext}.GetDbSet{TEntity}"></see> </para>
    /// 
    /// <para>   var groupByClause = from poco in dbSet                                                                             </para>
    ///                              group poco by new { poco.Id,  poco.PocoCode } into groupedPocos                                      
    /// <para>                       select new PocoDTO                                                                             </para>
    /// <para>                       {                                                                                              </para>
    /// <para>                            Id = groupedPocos.Key.Id,                                                                 </para>
    /// <para>                            PocoCode = groupedPocos.Key.PocoCode,                                                     </para>
    /// <para>                            PocoCount = groupedPocos.Sum(p=>p.Count)                                                  </para>
    /// <para>                       };                                                                                             </para>
    ///                        
    /// <para>   var result = await _pocoRepository.GetGroupedAsync{PocoDTO}(groupByClause).ConfigureAwait(false);                  </para>
    ///    
    /// </code>
    /// 
    /// </summary>
    /// 
    /// 
    /// 
    /// <typeparam name="TReturn"></typeparam>
    /// <param name="groupedClause"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<TReturn>> GetAsGroupedAsync<TReturn>(IQueryable<TReturn> groupedClause,
                                                                        Expression<Func<TReturn, bool>> conditionExpression = null,
                                                                        CancellationToken cancellationToken = new CancellationToken())
        => await groupedClause.Where(conditionExpression ?? (entity => true)).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets grouped entities with condition from database with <paramref name="groupedClause"/>.
    /// 
    /// <para> <b>Example use;</b></para>
    /// <code>
    /// 
    /// <para>   Func{IQueryablePocoDTO>} groupByClauseFunc = () =>   from poco in _contextRepository.GetDbSet{Poco}()                                       </para>
    ///                                                               group poco by new { poco.Id,  poco.PocoCode } into groupedPocos                                      
    /// <para>                                                        select new PocoDTO                                                                     </para>
    /// <para>                                                        {                                                                                      </para>
    /// <para>                                                             Id = groupedPocos.Key.Id,                                                         </para>
    /// <para>                                                             PocoCode = groupedPocos.Key.PocoCode,                                             </para>
    /// <para>                                                             PocoCount = groupedPocos.Sum(p=>p.Count)                                          </para>
    /// <para>                                                        };                                                                                     </para>
    ///                        
    /// <para>   var result = await _pocoRepository.GetGroupedAsync{PocoDTO}(groupByClauseFunc).ConfigureAwait(false);                                       </para>
    ///    
    /// </code>
    /// 
    /// </summary>
    /// 
    /// 
    /// 
    /// <typeparam name="TReturn"></typeparam>
    /// <param name="conditionExpression"></param>
    /// <param name="groupedClause"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<List<TReturn>> GetAsGroupedAsync<TReturn>(Func<IQueryable<TReturn>> groupedClause,
                                                                        Expression<Func<TReturn, bool>> conditionExpression = null,
                                                                        CancellationToken cancellationToken = new CancellationToken())
        => await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets grouped entities with condition from database with <paramref name="groupedClause"/>.
    /// 
    /// <para> <b>Example use;</b></para>
    /// <code>
    /// 
    /// <para>   Func{IQueryablePocoDTO>} groupByClauseFunc = () =>   from poco in _contextRepository.GetDbSet{Poco}()                                       </para>
    ///                                                               group poco by new { poco.Id,  poco.PocoCode } into groupedPocos                                      
    /// <para>                                                              select new PocoDTO                                                               </para>
    /// <para>                                                              {                                                                                </para>
    /// <para>                                                                   Id = groupedPocos.Key.Id,                                                   </para>
    /// <para>                                                                   PocoCode = groupedPocos.Key.PocoCode,                                       </para>
    /// <para>                                                                   PocoCount = groupedPocos.Sum(p=>p.Count)                                    </para>
    /// <para>                                                              };                                                                               </para>
    ///                        
    /// <para>   var result = await _pocoRepository.GetGroupedAsync{PocoDTO}(1, 10, groupByClauseFunc).ConfigureAwait(false);                                </para>
    ///    
    /// </code>
    /// 
    /// </summary>
    /// 
    /// 
    /// 
    /// <typeparam name="TReturn"></typeparam>
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="groupedClause"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAsync<TReturn>(int requestedPageNumber,
                                                                                                                     int countOfRequestedRecordsInPage,
                                                                                                                     Func<IQueryable<TReturn>> groupedClause,
                                                                                                                     Expression<Func<TReturn, bool>> conditionExpression = null,
                                                                                                                     CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync(cancellationToken).ConfigureAwait(false);

        var repo = await groupedClause.Invoke()
                                      .Where(conditionExpression ?? (entity => true))
                                      .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                      .Take(countOfRequestedRecordsInPage)
                                      .ToListAsync(cancellationToken)
                                      .ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages);
    }

    /// <summary>
    /// Gets grouped entities with condition from database with <paramref name="groupedClause"/>.
    /// 
    /// <para> <b>Example use;</b></para>
    /// <code>
    /// 
    /// <para>   Func{IQueryablePocoDTO>} groupByClauseFunc = () =>   from poco in _contextRepository.GetDbSet{Poco}()                                                    </para>
    ///                                                               group poco by new { poco.Id,  poco.PocoCode } into groupedPocos                                      
    /// <para>                                                              select new PocoDTO                                                                             </para>
    /// <para>                                                              {                                                                                              </para>
    /// <para>                                                                   Id = groupedPocos.Key.Id,                                                                 </para>
    /// <para>                                                                   PocoCode = groupedPocos.Key.PocoCode,                                                     </para>
    /// <para>                                                                   PocoCount = groupedPocos.Sum(p=>p.Count)                                                  </para>
    /// <para>                                                              };                                                                                             </para>
    ///                        
    /// <para>   var result = await _pocoRepository.GetGroupedAsync{PocoDTO}(1, 10, "PocoCode", false, groupByClauseFunc).ConfigureAwait(false);                           </para>
    ///    
    /// </code>
    /// 
    /// </summary>
    /// 
    /// 
    /// 
    /// <typeparam name="TReturn"></typeparam>
    /// <param name="requestedPageNumber"></param>
    /// <param name="countOfRequestedRecordsInPage"></param>
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="groupedClause"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAndOrderedAsync<TReturn>(int requestedPageNumber,
                                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                                               string orderByPropertyName,
                                                                                                                               bool orderByAscending,
                                                                                                                               Func<IQueryable<TReturn>> groupedClause,
                                                                                                                               Expression<Func<TReturn, bool>> conditionExpression = null,
                                                                                                                               CancellationToken cancellationToken = new CancellationToken())
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TReturn);

        CheckProperty(orderByPropertyName, entityType);

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

        var predicate = Expression.Lambda<Func<TReturn, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync(cancellationToken).ConfigureAwait(false);

        List<TReturn> repo;

        if (orderByAscending) repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                                 .OrderBy(predicate)
                                                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                 .Take(countOfRequestedRecordsInPage)
                                                                 .ToListAsync(cancellationToken).ConfigureAwait(false);
        else repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                .OrderByDescending(predicate)
                                                .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                .Take(countOfRequestedRecordsInPage)
                                                .ToListAsync(cancellationToken).ConfigureAwait(false);

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages);
    }

    /// <summary>
    /// Gets grouped entities with condition from database with <paramref name="groupedClause"/>.
    /// 
    /// <para> <b>Example use;</b></para>
    /// <code>
    /// 
    /// <para>   Func{IQueryablePocoDTO>} groupByClauseFunc = () =>   from poco in _contextRepository.GetDbSet{Poco}()                                       </para>
    ///                                                               group poco by new { poco.Id,  poco.PocoCode } into groupedPocos                                      
    /// <para>                                                        select new PocoDTO                                                                     </para>
    /// <para>                                                        {                                                                                      </para>
    /// <para>                                                             Id = groupedPocos.Key.Id,                                                         </para>
    /// <para>                                                             PocoCode = groupedPocos.Key.PocoCode,                                             </para>
    /// <para>                                                             PocoCount = groupedPocos.Sum(p=>p.Count)                                          </para>
    /// <para>                                                        };                                                                                     </para>
    ///                        
    /// <para>   var result = await _pocoRepository.GetGroupedAsync{PocoDTO}("PocoCode", false, groupByClauseFunc).ConfigureAwait(false);                    </para>
    ///    
    /// </code>
    /// 
    /// </summary>
    /// 
    /// 
    /// 
    /// <typeparam name="TReturn"></typeparam>
    /// <param name="orderByPropertyName"></param>
    /// <param name="orderByAscending"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="groupedClause"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<TReturn>> GetAsGroupedAndOrderedAsync<TReturn>(string orderByPropertyName,
                                                                                         bool orderByAscending,
                                                                                         Func<IQueryable<TReturn>> groupedClause,
                                                                                         Expression<Func<TReturn, bool>> conditionExpression = null,
                                                                                         CancellationToken cancellationToken = new CancellationToken())
    {
        var entityType = typeof(TReturn);

        CheckProperty(orderByPropertyName, entityType);

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

        var predicate = Expression.Lambda<Func<TReturn, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync(cancellationToken).ConfigureAwait(false);

        List<TReturn> repo;

        if (orderByAscending)
            repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                               .OrderBy(predicate)
                                               .ToListAsync(cancellationToken).ConfigureAwait(false);
        else
            repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                               .OrderByDescending(predicate)
                                               .ToListAsync(cancellationToken).ConfigureAwait(false);
        return repo;
    }

    /// <summary>
    /// Get max value of entities.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetMaxAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                   bool tracking = false,
                                                   CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression ?? (entity => entity))
                       .MaxAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Get max value of entities. With includes.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetMaxAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                   bool tracking = false,
                                                   CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .IncludeMultiple(includes)
                       .Select(projectionExpression ?? (entity => entity))
                       .MaxAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> GetMaxAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                              bool tracking = false,
                                                              CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .Select(projectionExpression ?? (entity => entity))
                       .OrderByDescending(maxProperty)
                       .FirstOrDefaultAsync(cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<object> GetMaxAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                                             Func<IIncludable<TEntity>, IIncludable> includes,
                                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                             bool tracking = false,
                                                             CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                        .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                        .IncludeMultiple(includes)
                        .Select(projectionExpression ?? (entity => entity))
                        .OrderByDescending(maxProperty)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxPropertyName"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<object> GetMaxOfPropertyAsync(string maxPropertyName,
                                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                                            bool tracking = false,
                                                            CancellationToken cancellationToken = new CancellationToken())
    {
        var entityType = typeof(TEntity);

        if (!CommonHelper.PropertyExists<TEntity>(maxPropertyName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{maxPropertyName}'.");

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, maxPropertyName);

        var predicate = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        return await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                            .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                            .MaxAsync(predicate, cancellationToken)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<object> GetMaxOfPropertyAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                                                       Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                       bool tracking = false,
                                                                       CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .MaxAsync(maxProperty, cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Gets sums of value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="sumProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<decimal> GetSumOfPropertyAsync(Expression<Func<TEntity, decimal>> sumProperty,
                                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                                             bool tracking = false,
                                                             CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .SumAsync(sumProperty, cancellationToken)
                       .ConfigureAwait(false);

    /// <summary>
    /// Get count of entities.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                                 bool tracking = false,
                                                 CancellationToken cancellationToken = new CancellationToken())
        => await _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                       .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                       .CountAsync(cancellationToken)
                       .ConfigureAwait(false);

    #endregion

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false) != null;

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        var mainCondition = CreateKeyEqualityExpression(id);

        return await _dbSet.AsNoTracking().AnyAsync(mainCondition).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual async Task<bool> ExistsAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return await _dbSet.AsNoTracking().IncludeMultiple(includes).AnyAsync(mainCondition).ConfigureAwait(false);
    }

    /// <summary>
    ///  Adds single entity to database asynchronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        if (SaveChangesAfterEveryTransaction) await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///  Adds multiple entities to database asynchronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.AddRange(entities);

            if (SaveChangesAfterEveryTransaction)
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///  Updates specified entity in database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        var dbEntry = _dbContext.Entry(entity);

        foreach (var includeProperty in projectionProperties)
            dbEntry.Property(includeProperty).IsModified = true;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        if (!entities.IsNullOrEmpty())
        {
            foreach (var entity in entities)
            {
                var dbEntry = _dbContext.Entry(entity);

                foreach (var includeProperty in projectionProperties)
                    dbEntry.Property(includeProperty).IsModified = true;
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    ///  Updates multiple entities in database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.UpdateRange(entities);

            if (SaveChangesAfterEveryTransaction)
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///  Deletes single entity from database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///  Deletes multiple entity from database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual async Task DeleteAsync(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.RemoveRange(entities);

            if (SaveChangesAfterEveryTransaction)
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual async Task ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        if (!oldEntities.IsNullOrEmpty())
        {
            _dbSet.RemoveRange(oldEntities);
        }

        if (!newEntities.IsNullOrEmpty())
        {
            _dbSet.AddRange(newEntities);
        }

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
    /// </summary>
    /// <param name="oldEntities"></param>
    /// <param name="newEntities"></param>
    /// <returns></returns>
    public virtual async Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
    {
        if (!oldEntities.IsNullOrEmpty())
            await DeleteAsync(oldEntities).ConfigureAwait(false);
        if (!newEntities.IsNullOrEmpty())
            await AddRangeAsync(newEntities).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes all entities from database.
    /// </summary>
    /// <returns></returns>
    public virtual async Task RemoveAllAsync()
    {
        var entities = _dbSet.AsEnumerable();

        _dbSet.RemoveRange(entities);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    #region Bulk Async

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task AddBulkAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
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
    public virtual async Task UpdateBulkAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
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
    public virtual async Task DeleteBulkAsync(List<TEntity> entities, Action<BulkConfig> bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
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
    public virtual async Task AddBulkWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.AddRange(entities);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task UpdateBulkWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.UpdateRange(entities);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task DeleteBulkWithSaveChangesAsync(List<TEntity> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        if (SaveChangesAfterEveryTransaction)
            await _dbContext.SaveChangesBulkAsync(bulkConfig, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates Id == <typeparamref name="TKey"/> equality expression and append to <see cref="CommonHelper.CreateIsDeletedFalseExpression"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected static Expression<Func<TEntity, bool>> CreateKeyEqualityExpression(TKey key, Expression<Func<TEntity, bool>> conditionExpression = null)
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
        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

        return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);
    }

    /// <summary>
    /// Checks if there is a property named <paramref name="propertyName"/> in the properties of <b><paramref name="entityType"/></b>. 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="entityType"></param>
    protected static void CheckProperty(string propertyName, Type entityType)
    {
        if (!CommonHelper.PropertyExists<TEntity>(propertyName))
            throw new MilvaDeveloperException($"Type of {entityType.Name}'s properties doesn't contain '{propertyName}'.");
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
        if (requestedPageNumber <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

        if (countOfRequestedRecordsInPage <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);
    }

    /// <summary>
    /// If <see cref="_getSoftDeletedEntities"/> is false,  appends is deleted false expression to <paramref name="conditionExpression"/>.
    /// Else does nothing to <paramref name="conditionExpression"/> but if <see cref="_resetGetSoftDeletedEntitiesState"/> is true then sets <see cref="_getSoftDeletedEntities"/> false.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    protected static Expression<Func<TEntity, bool>> CreateConditionExpression(Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        Expression<Func<TEntity, bool>> mainExpression;

        //Step in when _softDeleteState is false
        if (!_getSoftDeletedEntities)
        {
            var softDeleteExpression = CommonHelper.CreateIsDeletedFalseExpression<TEntity>();

            mainExpression = softDeleteExpression.Append(conditionExpression, ExpressionType.AndAlso);
        }
        else
        {
            mainExpression = conditionExpression;

            if (_resetGetSoftDeletedEntitiesState)
                _getSoftDeletedEntities = false;
        }

        return mainExpression;
    }

    /// <summary>
    /// Returns <see cref="QueryTrackingBehavior"/> according to <paramref name="tracking"/>.
    /// </summary>
    /// <param name="tracking"></param>
    /// <returns></returns>
    protected static QueryTrackingBehavior GetQueryTrackingBehavior(bool tracking) => tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking;

    /// <summary>
    /// For configure entity state. Change tracking.
    /// </summary>
    /// <param name="entity"></param>
    protected void InitalizeEdit(TEntity entity)
    {
        if (InitializeUpdate)
        {
            var local = _dbSet.Local.FirstOrDefault(entry => entry.Id.Equals(entity.Id));
            if (local != null)
            {
                _dbContext.Entry(local).State = EntityState.Detached;
            }
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
    }

    /// <summary>
    /// For configure entity state. Change tracking.
    /// </summary>
    /// <param name="entities"></param>
    protected void InitalizeEdit(IEnumerable<TEntity> entities)
    {
        if (InitializeUpdate)
        {
            var localEntities = _dbSet.Local.Where(e => entities.Any(en => en.Id.Equals(e.Id)));
            if (!localEntities?.Any() ?? false)
            {
                foreach (var entity in localEntities)
                    _dbContext.Entry(entity).State = EntityState.Detached;
            }
            foreach (var entity in entities)
                _dbContext.Entry(entity).State = EntityState.Modified;
        }
    }

    #endregion
}