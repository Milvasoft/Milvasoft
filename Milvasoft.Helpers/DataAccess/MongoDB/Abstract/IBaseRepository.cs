using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.MongoDB.Abstract
{
    /// <summary>
    /// Base repository for concrete repositories. All repositories must be have this methods.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IBaseRepository<TEntity> where TEntity : class, IAuditable<ObjectId>
    {
        IMongoDatabase GetMongoDatabaseInstance();

        /// <summary>
        /// Returns all entities.
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="projectExpression"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filterExpression = null, Expression<Func<TEntity, TEntity>> projectExpression = null);

        /// <summary>
        /// Returns all entities.
        /// </summary>
        /// <param name="filterDefinition"></param>
        /// <param name="projectExpression"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetAllAsync(FilterDefinition<TEntity> filterDefinition, Expression<Func<TEntity, TEntity>> projectExpression = null);

        /// <summary>
        /// Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projectExpression"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetByIdAsync(ObjectId id, Expression<Func<TEntity, TEntity>> projectExpression = null);

        /// <summary>
        /// Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projectExpression"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetFirstOrDefaultAsync(FilterDefinition<TEntity> filter = null, Expression<Func<TEntity, TEntity>> projectExpression = null);


        /// <summary>
        /// Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projectExpression"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> projectExpression = null);

        /// <summary>
        /// Returns all entities.
        /// </summary>
        /// <param name="filterDefinition"></param>
        /// <param name="propertySelector"></param>
        /// <param name="projectExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync(FilterDefinition<TEntity> filterDefinition, Expression<Func<TEntity, object>> propertySelector, Expression<Func<TEntity, TEntity>> projectExpression = null);

        /// <summary>
        /// Returns entity count.
        /// </summary>
        /// <returns></returns>
        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filterExpression = null);

        /// <summary>
        /// Returns embedded document count.
        /// </summary>
        /// <typeparam name="TEmbedded"></typeparam>
        /// <param name="unwindExpression"></param>
        /// <param name="filterDefinition"></param>
        /// <returns></returns>
        Task<int> GetEmbeddedDocumentCountAsync<TEmbedded>(Expression<Func<TEntity, object>> unwindExpression, FilterDefinition<TEmbedded> filterDefinition = null);

        /// <summary>
        /// Returns the nested list in the entity according to <paramref name="entityId"/>.
        /// 
        /// <remarks>
        /// <para><b>Remarks ;</b></para>
        /// 
        /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
        /// <para> You can send the filter value to the Nested list. See <paramref name="filterExpressionForTEmbedded"/>. </para>
        /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
        /// </remarks>
        /// </summary>
        /// <typeparam name="TEmbedded"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="unwindExpression"></param>
        /// <param name="filterExpressionForTEmbedded"></param>
        /// <param name="projectExpression"></param>
        /// <returns></returns>
        Task<List<TEmbedded>> GetNestedArrayByEntityIdAsync<TEmbedded>(ObjectId entityId,
                                                                       Expression<Func<TEntity, object>> unwindExpression,
                                                                       Expression<Func<TEmbedded, bool>> filterExpressionForTEmbedded = null,
                                                                       List<Expression<Func<TEmbedded, object>>> projectExpression = null);

        /// <summary>
        /// 
        /// You can bring up the page number you want with the number of data count you want.
        /// 
        /// <para> You can send the filter value to the Nested list. See <paramref name="filterDefinition"/>. </para>
        /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="filterDefinition"></param>
        /// <param name="projectExpression"></param>
        /// <returns></returns>
        Task<(List<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int pageIndex,
                                                                                              int requestedItemCount,
                                                                                              string orderByProperty,
                                                                                              bool orderByAscending,
                                                                                              FilterDefinition<TEntity> filterDefinition = null,
                                                                                              Expression<Func<TEntity, TEntity>> projectExpression = null);
        /// <summary>
        /// 
        /// According to the <paramref name="entityId"/>, you can bring up the nested list with the page number you want and the number of data you want.
        /// 
        /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
        /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
        /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
        /// 
        /// </summary>
        /// <typeparam name="TEmbedded"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="unwindExpression"></param>
        /// <param name="projectExpression"></param>
        /// <param name="filterDefinition"></param>
        /// <returns></returns>
        Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(ObjectId entityId,
                                                                                                                         int pageIndex,
                                                                                                                         int requestedItemCount,
                                                                                                                         string orderByProperty,
                                                                                                                         bool orderByAscending,
                                                                                                                         Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                         List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                         FilterDefinition<TEmbedded> filterDefinition = null);

        /// <summary>
        /// 
        /// According to the <paramref name="entityIds"/>, you can bring up the nested list with the page number you want and the number of data you want.
        /// 
        /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
        /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
        /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
        /// 
        /// </summary>
        /// <typeparam name="TEmbedded"></typeparam>
        /// <param name="entityIds"></param>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="unwindExpression"></param>
        /// <param name="projectExpression"></param>
        /// <param name="filterDefinition"></param>
        /// <returns></returns>
        Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(List<ObjectId> entityIds,
                                                                                                                         int pageIndex,
                                                                                                                         int requestedItemCount,
                                                                                                                         string orderByProperty,
                                                                                                                         bool orderByAscending,
                                                                                                                         Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                         List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                         FilterDefinition<TEmbedded> filterDefinition = null);

        /// <summary>
        /// 
        /// You can bring up the nested list with the page number you want and the number of data you want.
        /// 
        /// <para> You can send the filter value to the Nested list. See <paramref name="projectExpression"/>. </para>
        /// <para> You can get the specific properties. See <paramref name="projectExpression"/>. </para>
        /// <para> We specify the nested list with <paramref name="unwindExpression"/>. </para>
        /// 
        /// </summary>
        /// <typeparam name="TEmbedded"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="requestedItemCount"></param>
        /// <param name="orderByProperty"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="unwindExpression"></param>
        /// <param name="filterExpression"></param>
        /// <param name="projectExpression"></param>
        /// <param name="filterDefForTEmbedded"></param>
        /// <returns></returns>
        Task<(List<TEmbedded> entities, int pageCount, int totalDataCount)> GetNestedPropertyAsPaginatedAsync<TEmbedded>(int pageIndex,
                                                                                                                         int requestedItemCount,
                                                                                                                         string orderByProperty,
                                                                                                                         bool orderByAscending,
                                                                                                                         Expression<Func<TEntity, object>> unwindExpression,
                                                                                                                         FilterDefinition<TEntity> filterExpression = null,
                                                                                                                         List<Expression<Func<TEmbedded, object>>> projectExpression = null,
                                                                                                                         FilterDefinition<TEmbedded> filterDefForTEmbedded = null);

        /// <summary>
        ///  Adds single entity to database asynchronously.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        Task AddAsync(TEntity document);

        /// <summary>
        ///  Adds multiple entities to database asynchronously.
        /// </summary>
        /// <param name="documents"></param>
        /// <returns></returns>
        Task AddAsync(ICollection<TEntity> documents);

        /// <summary>
        /// Replaces existing entities with new entities(<paramref name="document"/>).
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity document);

        /// <summary>
        /// Updates existing assets according to <paramref name="updateDefinition"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="updateDefinition"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity document, UpdateDefinition<TEntity> updateDefinition);

        /// <summary>
        /// Updates existing assets according to <paramref name="updateDefinition"/> matching <paramref name="filterDefinition"/>.
        /// </summary>
        /// <param name="filterDefinition"></param>
        /// <param name="updateDefinition"></param>
        /// <returns></returns>
        Task UpdateAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition);

        /// <summary>
        /// Updates the data in multiple.
        /// 
        /// <para> You can only update one property in multiple. See <paramref name="fieldDefinition"/> </para>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="fieldDefinition"></param>
        /// <returns></returns>
        Task UpdateRangeAsync(List<TEntity> entities, params Expression<Func<TEntity, object>>[] fieldDefinition);

        /// <summary>
        ///  Deletes multiple entity from database asynchronously. 
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task DeleteRangeAsync(FilterDefinition<TEntity> filterExpression);

        /// <summary>
        ///  Deletes single entity from database asynchronously..
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(ObjectId id);

        /// <summary>
        ///  Deletes single entity from database asynchronously..
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task DeleteAsync(Expression<Func<TEntity, bool>> filterExpression);
    }
}
