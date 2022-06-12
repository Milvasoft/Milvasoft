using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core;
using Milvasoft.Core.EntityBase.Abstract;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.DataAccess.EfCore.Concrete;
using Milvasoft.DataAccess.EfCore.IncludeLibrary;
using Milvasoft.DataAccess.EfCore.MilvaContext;
using System.Linq.Expressions;

namespace Milvasoft.Helpers.DataAccess.EfCore.Concrete;

/// <summary>
///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
/// </summary>
public abstract partial class BaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                                      where TKey : struct, IEquatable<TKey>
                                                                      where TContext : DbContext, IMilvaDbContextBase
{
    #region Sync

    #region Get Data  

    /// <summary>
    ///  Returns first entity or default value which IsDeleted condition is true from database hronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Select(projectionExpression ?? (entity => entity))
                 .FirstOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));


    /// <summary>
    ///  Returns first entity or default value which IsDeleted condition is true with includes from database hronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetFirstOrDefault(Func<IIncludable<TEntity>, IIncludable> includes,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .FirstOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true from database hronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> conditionExpression = null,
                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Select(projectionExpression ?? (entity => entity))
                 .SingleOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    /// <summary>
    ///  Returns single entity or default value which IsDeleted condition is true with includes from database hronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TEntity GetSingleOrDefault(Func<IIncludable<TEntity>, IIncludable> includes,
                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .SingleOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true));

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> conditionExpression = null,
                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                               bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    /// <summary>
    /// Returns all entities which IsDeleted condition is true from database synchronously. If the condition is requested, it also provides that condition.
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
    ///  Returns all entities which IsDeleted condition is true with specified includes from database hronously. If the condition is requested, it also provides that condition.
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
    /// Returns all entities which IsDeleted condition is true with specified includes from database hronously. If the condition is requested, it also provides that condition.
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

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true from database hronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetSome(int count,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    /// <summary>
    ///  Returns all entities which IsDeleted condition is true with specified includes from database hronously. If the condition is requested, it also provides that condition.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="includes"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetSome(int count,
                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Take(count)
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .ToList();

    #region Pagination And Order

    /// <summary>
    ///  Creates hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range. 
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginated(int requestedPageNumber,
                                                                                                                 int countOfRequestedRecordsInPage,
                                                                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                 bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        var repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                         .Where(condition ?? (entity => true))
                         .Select(projectionExpression ?? (entity => entity))
                         .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                         .Take(countOfRequestedRecordsInPage)
                         .ToList();

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginated(int requestedPageNumber,
                                                                                                                 int countOfRequestedRecordsInPage,
                                                                                                                 Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                 Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                 Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                 bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        var repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                         .Where(condition ?? (entity => true))
                         .IncludeMultiple(includes)
                         .Select(projectionExpression ?? (entity => entity))
                         .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                         .Take(countOfRequestedRecordsInPage)
                         .ToList();

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates and ordered hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrdered(int requestedPageNumber,
                                                                                                                           int countOfRequestedRecordsInPage,
                                                                                                                           string orderByPropertyName,
                                                                                                                           bool orderByAscending,
                                                                                                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                           bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .Select(projectionExpression ?? (entity => entity))
                                           .OrderBy(predicate)
                                           .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                           .Take(countOfRequestedRecordsInPage)
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .Select(projectionExpression ?? (entity => entity))
                          .OrderByDescending(predicate)
                          .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                          .Take(countOfRequestedRecordsInPage)
                          .ToList();

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates and ordered hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrdered(int requestedPageNumber,
                                                                                                                           int countOfRequestedRecordsInPage,
                                                                                                                           Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                           string orderByPropertyName,
                                                                                                                           bool orderByAscending,
                                                                                                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                           bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                                  .Where(condition ?? (entity => true))
                                                  .OrderBy(predicate)
                                                  .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                  .Take(countOfRequestedRecordsInPage)
                                                  .IncludeMultiple(includes)
                                                  .Select(projectionExpression ?? (entity => entity))
                                                  .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                 .Where(condition ?? (entity => true))
                                 .OrderByDescending(predicate)
                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                 .Take(countOfRequestedRecordsInPage)
                                 .IncludeMultiple(includes)
                                 .Select(projectionExpression ?? (entity => entity))
                                 .ToList();

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrdered(int requestedPageNumber,
                                                                                                                           int countOfRequestedRecordsInPage,
                                                                                                                           Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                           bool orderByAscending,
                                                                                                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                           bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .Select(projectionExpression ?? (entity => entity))
                                           .OrderBy(orderByKeySelector)
                                           .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                           .Take(countOfRequestedRecordsInPage)
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .Select(projectionExpression ?? (entity => entity))
                          .OrderByDescending(orderByKeySelector)
                          .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                          .Take(countOfRequestedRecordsInPage)
                          .ToList();

        var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

        return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount);
    }

    /// <summary>
    ///  Creates hronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrdered(int requestedPageNumber,
                                                                                                                           int countOfRequestedRecordsInPage,
                                                                                                                           Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                           Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                           bool orderByAscending,
                                                                                                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                                                                                           bool tracking = false)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var condition = CreateConditionExpression(conditionExpression);

        var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .OrderBy(orderByKeySelector)
                                           .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                           .Take(countOfRequestedRecordsInPage)
                                           .IncludeMultiple(includes)
                                           .Select(projectionExpression ?? (entity => entity))
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .OrderByDescending(orderByKeySelector)
                          .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                          .Take(countOfRequestedRecordsInPage)
                          .IncludeMultiple(includes)
                          .Select(projectionExpression ?? (entity => entity))
                          .ToList();

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
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAsOrdered(string orderByPropertyName,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                     bool tracking = false)
    {
        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .Select(projectionExpression ?? (entity => entity))
                                           .OrderBy(predicate)
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .Select(projectionExpression ?? (entity => entity))
                          .OrderByDescending(predicate)
                          .ToList();

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
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAsOrdered(Func<IIncludable<TEntity>, IIncludable> includes,
                                                     string orderByPropertyName,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                     bool tracking = false)
    {
        var entityType = typeof(TEntity);

        CheckProperty(orderByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .OrderBy(predicate)
                                           .IncludeMultiple(includes)
                                           .Select(projectionExpression ?? (entity => entity))
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .OrderByDescending(predicate)
                          .IncludeMultiple(includes)
                          .Select(projectionExpression ?? (entity => entity))
                          .ToList();

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
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAsOrdered(Expression<Func<TEntity, object>> orderByKeySelector,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                     bool tracking = false)
    {
        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .Select(projectionExpression ?? (entity => entity))
                                           .OrderBy(orderByKeySelector)
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .Select(projectionExpression ?? (entity => entity))
                          .OrderByDescending(orderByKeySelector)
                          .ToList();

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
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetAsOrdered(Func<IIncludable<TEntity>, IIncludable> includes,
                                                     Expression<Func<TEntity, object>> orderByKeySelector,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                     bool tracking = false)
    {
        var condition = CreateConditionExpression(conditionExpression);

        List<TEntity> repo;

        if (orderByAscending) repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                                           .Where(condition ?? (entity => true))
                                           .OrderBy(orderByKeySelector)
                                           .IncludeMultiple(includes)
                                           .Select(projectionExpression ?? (entity => entity))
                                           .ToList();
        else repo = _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                          .Where(condition ?? (entity => true))
                          .OrderByDescending(orderByKeySelector)
                          .IncludeMultiple(includes)
                          .Select(projectionExpression ?? (entity => entity))
                          .ToList();

        return repo;
    }

    #endregion

    /// <summary>
    /// Returns one entity by entity Id from database hronously.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TEntity GetById(TKey id,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                   bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Select(projectionExpression ?? (entity => entity))
                     .SingleOrDefault(mainCondition);
    }

    /// <summary>
    ///  Returns one entity by entity Id from database hronously.
    /// </summary>
    /// 
    /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
    /// 
    /// <param name="id"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity. </returns>
    public virtual TEntity GetRequiredById(TKey id,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                           bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Select(projectionExpression ?? (entity => entity))
                     .Single(mainCondition);
    }

    /// <summary>
    ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database hronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity found or null. </returns>
    public virtual TEntity GetById(TKey id,
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


    /// <summary>
    ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database hronously. If the condition is requested, it also provides that condition. 
    /// </summary>
    /// 
    /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
    /// 
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns> The entity. </returns>
    public virtual TEntity GetRequiredById(TKey id,
                                           Func<IIncludable<TEntity>, IIncludable> includes,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                           bool tracking = false)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .IncludeMultiple(includes)
                     .Select(projectionExpression ?? (entity => entity))
                     .Single(mainCondition);
    }

    /// <summary>
    /// Groups entities with <paramref name="groupByPropertyName"/> and returns the key grouped and the number of items grouped with this key.
    /// </summary>
    /// <param name="groupByPropertyName"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual List<Tuple<object, int>> GetGroupedAndCount(string groupByPropertyName,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false)
    {
        var entityType = typeof(TEntity);

        CheckProperty(groupByPropertyName, entityType);

        var predicate = CreateObjectPredicate(entityType, groupByPropertyName);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                     .GroupBy(predicate)
                     .Select(b => Tuple.Create(b.Key, b.Count()))
                     .ToList();
    }

    /// <summary>
    /// Groups entities with <paramref name="keySelector"/> and returns the key grouped and the number of items grouped with this key.
    /// </summary>
    /// <param name="keySelector"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual List<Tuple<object, int>> GetGroupedAndCount(Expression<Func<TEntity, object>> keySelector,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .GroupBy(keySelector)
                 .Select(b => Tuple.Create(b.Key, b.Count()))
                 .ToList();

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
    /// <para>   var result =  _pocoRepository.GetGrouped{PocoDTO}(groupByClause);                  </para>
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
    /// <returns></returns>
    public virtual List<TReturn> GetAsGrouped<TReturn>(IQueryable<TReturn> groupedClause, Expression<Func<TReturn, bool>> conditionExpression = null)
        => groupedClause.Where(conditionExpression ?? (entity => true)).ToList();

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
    /// <para>   var result =  _pocoRepository.GetGrouped{PocoDTO}(groupByClauseFunc);                                       </para>
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
    /// <returns></returns>
    public virtual List<TReturn> GetAsGrouped<TReturn>(Func<IQueryable<TReturn>> groupedClause, Expression<Func<TReturn, bool>> conditionExpression = null)
        => groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).ToList();

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
    /// <para>   var result =  _pocoRepository.GetGrouped{PocoDTO}(1, 10, groupByClauseFunc);                                </para>
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginated<TReturn>(int requestedPageNumber,
                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                Func<IQueryable<TReturn>> groupedClause,
                                                                                                                Expression<Func<TReturn, bool>> conditionExpression = null)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync();

        var repo = groupedClause.Invoke()
                                .Where(conditionExpression ?? (entity => true))
                                .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                .Take(countOfRequestedRecordsInPage)
                                .ToList();

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
    /// <para>   var result =  _pocoRepository.GetGrouped{PocoDTO}(1, 10, "PocoCode", false, groupByClauseFunc);                           </para>
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
    /// <returns></returns>
    public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAndOrdered<TReturn>(int requestedPageNumber,
                                                                                                                          int countOfRequestedRecordsInPage,
                                                                                                                          string orderByPropertyName,
                                                                                                                          bool orderByAscending,
                                                                                                                          Func<IQueryable<TReturn>> groupedClause,
                                                                                                                          Expression<Func<TReturn, bool>> conditionExpression = null)
    {
        ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

        var entityType = typeof(TReturn);

        if (!CommonHelper.PropertyExists<TReturn>(orderByPropertyName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{orderByPropertyName}'.");

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

        var predicate = Expression.Lambda<Func<TReturn, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync();

        List<TReturn> repo;

        if (orderByAscending) repo = groupedClause.Invoke()
                                                  .Where(conditionExpression ?? (entity => true))
                                                  .OrderBy(predicate)
                                                  .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                  .Take(countOfRequestedRecordsInPage)
                                                  .ToList();
        else repo = groupedClause.Invoke()
                                 .Where(conditionExpression ?? (entity => true))
                                 .OrderByDescending(predicate)
                                 .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                 .Take(countOfRequestedRecordsInPage)
                                 .ToList();

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
    /// <para>   var result =  _pocoRepository.GetGrouped{PocoDTO}("PocoCode", false, groupByClauseFunc);                    </para>
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
    /// <returns></returns>
    public virtual async Task<IEnumerable<TReturn>> GetAsGroupedAndOrdered<TReturn>(string orderByPropertyName,
                                                                                    bool orderByAscending,
                                                                                    Func<IQueryable<TReturn>> groupedClause,
                                                                                    Expression<Func<TReturn, bool>> conditionExpression = null)
    {
        var entityType = typeof(TReturn);

        if (!CommonHelper.PropertyExists<TReturn>(orderByPropertyName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{orderByPropertyName}'.");

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, orderByPropertyName);

        var predicate = Expression.Lambda<Func<TReturn, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync();

        List<TReturn> repo;

        if (orderByAscending)
            repo = groupedClause.Invoke()
                                .Where(conditionExpression ?? (entity => true))
                                .OrderBy(predicate)
                                .ToList();
        else
            repo = groupedClause.Invoke()
                                .Where(conditionExpression ?? (entity => true))
                                .OrderByDescending(predicate)
                                .ToList();
        return repo;
    }

    /// <summary>
    /// Get max value of entities.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TEntity GetMax(Expression<Func<TEntity, bool>> conditionExpression = null,
                                  Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                  bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression ?? (entity => entity))
                 .Max();

    /// <summary>
    /// Get max value of entities. With includes.
    /// </summary>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TEntity GetMax(Func<IIncludable<TEntity>, IIncludable> includes,
                                  Expression<Func<TEntity, bool>> conditionExpression = null,
                                  Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                  bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .Max();

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual TEntity GetMax<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Select(projectionExpression ?? (entity => entity))
                 .OrderByDescending(maxProperty)
                 .FirstOrDefault();

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="projectionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual object GetMax<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                            Func<IIncludable<TEntity>, IIncludable> includes,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                            bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .IncludeMultiple(includes)
                 .Select(projectionExpression ?? (entity => entity))
                 .OrderByDescending(maxProperty)
                 .FirstOrDefault();

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxPropertyName"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual object GetMaxOfProperty(string maxPropertyName,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           bool tracking = false)
    {
        var entityType = typeof(TEntity);

        if (!CommonHelper.PropertyExists<TEntity>(maxPropertyName))
            throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{maxPropertyName}'.");

        var parameterExpression = Expression.Parameter(entityType, "i");

        Expression orderByProperty = Expression.Property(parameterExpression, maxPropertyName);

        var predicate = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

        return _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                     .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                     .Max(predicate);
    }

    /// <summary>
    /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="maxProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual object GetMaxOfProperty<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                      bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Max(maxProperty);

    /// <summary>
    /// Gets sums of value of <typeparamref name="TEntity"/>'s property in entities.
    /// </summary>
    /// <param name="sumProperty"></param>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual decimal GetSumOfProperty(Expression<Func<TEntity, decimal>> sumProperty,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Sum(sumProperty);

    /// <summary>
    /// Get count of entities.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <param name="tracking"></param>
    /// <returns></returns>
    public virtual int GetCount(Expression<Func<TEntity, bool>> conditionExpression = null, bool tracking = false)
        => _dbSet.AsTracking(GetQueryTrackingBehavior(tracking))
                 .Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                 .Count();

    #endregion

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists.
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual bool Exists(Expression<Func<TEntity, bool>> conditionExpression = null)
        => _dbSet.AsNoTracking().FirstOrDefault(CreateConditionExpression(conditionExpression) ?? (entity => true)) != null;

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual bool Exists(TKey id)
    {
        var mainCondition = CreateKeyEqualityExpression(id);

        return _dbSet.AsNoTracking().Any(mainCondition);
    }

    /// <summary>
    /// Returns whether or not the entity that satisfies this condition exists. 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="conditionExpression"></param>
    /// <returns></returns>
    public virtual bool Exists(TKey id, Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
    {
        var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

        return _dbSet.AsNoTracking().IncludeMultiple(includes).Any(mainCondition);
    }

    /// <summary>
    ///  Adds single entity to database hronously. 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChanges();
    }

    /// <summary>
    ///  Adds multiple entities to database hronously. 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.AddRange(entities);

            if (SaveChangesAfterEveryTransaction)
                _dbContext.SaveChanges();
        }
    }

    /// <summary>
    ///  Updates specified entity in database hronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChanges();
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

        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Specific properties updates.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="projectionProperties"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] projectionProperties)
    {
        if (!entities.IsNullOrEmpty())
        {
            foreach (var entity in entities)
            {
                var dbEntry = _dbContext.Entry(entity);

                foreach (var includeProperty in projectionProperties)
                    dbEntry.Property(includeProperty).IsModified = true;
            }

            _dbContext.SaveChanges();
        }
    }

    /// <summary>
    ///  Updates multiple entities in database hronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void Update(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.UpdateRange(entities);

            if (SaveChangesAfterEveryTransaction)
                _dbContext.SaveChanges();
        }
    }

    /// <summary>
    ///  Deletes single entity from database hronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChanges();
    }

    /// <summary>
    ///  Deletes multiple entity from database hronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual void Delete(IEnumerable<TEntity> entities)
    {
        if (!entities.IsNullOrEmpty())
        {
            _dbSet.RemoveRange(entities);

            if (SaveChangesAfterEveryTransaction)
                _dbContext.SaveChanges();
        }
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
        {
            _dbSet.RemoveRange(oldEntities);
        }

        if (!newEntities.IsNullOrEmpty())
        {
            _dbSet.AddRange(newEntities);
        }

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChanges();
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

        _dbSet.RemoveRange(entities);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChanges();
    }

    #region Bulk Sync

    /// <summary>
    /// Bulk add operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void AddBulk(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
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
    public virtual void UpdateBulk(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
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
    public virtual void DeleteBulk(List<TEntity> entities, Action<BulkConfig> bulkConfig = null)
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
    public virtual void AddBulkWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.AddRange(entities);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChangesBulk(bulkConfig);
    }

    /// <summary>
    /// Bulk update operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    public virtual void UpdateBulkWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.UpdateRange(entities);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChangesBulk(bulkConfig);
    }

    /// <summary>
    /// Bulk delete operation. 
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="bulkConfig"></param>
    /// <returns></returns>
    public virtual void DeleteBulkWithSaveChanges(List<TEntity> entities, BulkConfig bulkConfig = null)
    {
        if (!entities.IsNullOrEmpty())
            _dbSet.RemoveRange(entities);

        if (SaveChangesAfterEveryTransaction)
            _dbContext.SaveChangesBulk(bulkConfig);
    }

    #endregion

    #endregion
}
