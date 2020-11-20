using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Abstract
{
    /// <summary>
    /// <para>  Base repository for concrete repositories. All repositories must be have this methods. </para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface IBaseRepository<TEntity, TKey, TContext> where TEntity : IBaseEntity<TKey>
                                                              where TKey : IEquatable<TKey>
                                                              where TContext : DbContext
    {

        /// <summary>
        /// <para> Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range. 
        ///       If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
        /// 
        /// <param name="requestedPageNumber"></param>
        /// <param name="countOfRequestedRecordsInPage"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                     int countOfRequestedRecordsInPage,
                                                                                                     Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
        ///        If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
        ///
        /// <param name="requestedPageNumber"></param>
        /// <param name="countOfRequestedRecordsInPage"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                     int countOfRequestedRecordsInPage,
                                                                                                     Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                     Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
        ///       If the condition is requested, it also provides that condition.</para> 
        ///       
        /// </summary>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        /// <exception cref="ArgumentNullException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
        /// 
        /// <param name="requestedPageNumber"></param>
        /// <param name="countOfRequestedRecordsInPage"></param>
        /// <param name="orderByPropertyName"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               string orderByPropertyName,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
        ///        If the condition is requested, it also provides that condition.</para> 
        ///        
        /// </summary>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        /// <exception cref="ArgumentNullException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
        ///
        /// <param name="requestedPageNumber"></param>
        /// <param name="countOfRequestedRecordsInPage"></param>
        /// <param name="includes"></param>
        /// <param name="orderByPropertyName"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                               string orderByPropertyName,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Gets entities as ordered with <paramref name="orderByPropertyName"/>.
        ///       If the condition is requested, it also provides that condition.</para> 
        ///       
        /// </summary>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        /// 
        /// <param name="orderByPropertyName"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(string orderByPropertyName,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Gets entities as ordered with <paramref name="orderByPropertyName"/>.
        ///        If the condition is requested, it also provides that condition.</para> 
        ///        
        /// </summary>
        /// 
        /// <exception cref="ArgumentException"> Throwns when type of <typeparamref name="TEntity"/>'s properties doesn't contain '<paramref name="orderByPropertyName"/>'. </exception>
        ///
        /// <param name="includes"></param>
        /// <param name="orderByPropertyName"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                          string orderByPropertyName,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// <para> Returns one entity by entity Id from database asynchronously.</para> 
        /// </summary>
        /// <param name="id"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetByIdAsync(TKey id);

        /// <summary>
        /// <para> Returns one entity by entity Id from database asynchronously. </para> 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
        /// 
        /// <param name="id"></param>
        /// <returns> The entity. </returns>
        Task<TEntity> GetRequiredByIdAsync(TKey id);

        /// <summary>
        /// <para> Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetByIdAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes);

        /// <summary>
        /// <para> Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
        /// 
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns> The entity. </returns>
        Task<TEntity> GetRequiredByIdAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes);

        /// <summary>
        /// <para> Adds single entity to database asynchronously.</para> 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// <para> Adds multiple entities to database asynchronously.</para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// <para> Updates specified entity in database asynchronously.</para> 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// <para> Updates multiple entities in database asynchronously.</para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task UpdateAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// <para> Deletes single entity from database asynchronously..</para> 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// <para> Deletes multiple entity from database asynchronously. </para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task DeleteAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Groups entities with <paramref name="groupByPropertyName"/> and returns the key grouped and the number of items grouped with this key.
        /// </summary>
        /// <param name="groupByPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(string groupByPropertyName, Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// Get max value of entities.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync(Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// Get max value of entities. With includes.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="groupByPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<object> GetMaxAsync(string groupByPropertyName, Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities. With includes.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="groupByPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<object> GetMaxAsync(Func<IIncludable<TEntity>, IIncludable> includes, string groupByPropertyName, Expression<Func<TEntity, bool>> conditionExpression = null);


    }
}
