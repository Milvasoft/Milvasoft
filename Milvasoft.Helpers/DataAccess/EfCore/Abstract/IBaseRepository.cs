using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Concrete;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Abstract
{
    /// <summary>
    /// Base repository for concrete repositories. All repositories must be have this methods.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface IBaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                              where TKey : struct, IEquatable<TKey>
                                                              where TContext : DbContext
    {
        /// <summary>
        /// Determines whether soft deleted entities in the database are fetched from the database.
        /// <para><b>Default is false.</b></para>
        /// </summary>
        /// <param name="state"></param>
        void GetSoftDeletedEntitiesInNextProcess(bool state);

        /// <summary>
        /// Determines whether the default value of the variable that determines the status of deleted data in the database is assigned to the default value after database operation.
        /// </summary>
        /// <param name="state"></param>
        void ResetSoftDeleteState(bool state);

        /// <summary>
        /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="FullAuditableEntity{TKey}"/>.
        /// </summary>
        /// <returns></returns>
        Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression();

        #region Get Data  

        /// <summary>
        ///  Returns first entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false);

        /// <summary>
        ///  Returns first entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition. 
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetFirstOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false);

        /// <summary>
        ///  Returns single entity or default value which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              bool tracking = false);

        /// <summary>
        ///  Returns single entity or default value which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<TEntity> GetSingleOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                              Expression<Func<TEntity, bool>> conditionExpression = null,
                                              Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                              bool tracking = false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                               bool tracking = false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                               Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                               bool tracking = false);
        /// <summary>
        ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetSomeAsync(int count,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                bool tracking = false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="includes"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetSomeAsync(int count,
                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                Expression<Func<TEntity, bool>> conditionExpression = null,
                                                Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                                bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                     int countOfRequestedRecordsInPage,
                                                                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                     bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                     int countOfRequestedRecordsInPage,
                                                                                                     Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                     bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               string orderByPropertyName,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                               bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                               string orderByPropertyName,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                               bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                               bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                               int countOfRequestedRecordsInPage,
                                                                                                               Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                               Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                               bool orderByAscending,
                                                                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                                                                               bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(string orderByPropertyName,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                     string orderByPropertyName,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     bool tracking = false);

        /// <summary>
        ///  Gets entities as ordered with <paramref name="orderByKeySelector"/>.
        ///       If the condition is requested, it also provides that condition.
        ///       
        /// </summary>
        /// 
        /// <param name="orderByKeySelector"></param>
        /// <param name="orderByAscending"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(Expression<Func<TEntity, object>> orderByKeySelector,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     bool tracking = false);

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
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                     Expression<Func<TEntity, object>> orderByKeySelector,
                                                     bool orderByAscending,
                                                     Expression<Func<TEntity, bool>> conditionExpression = null,
                                                     bool tracking = false);

        #endregion

        /// <summary>
        /// Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetByIdAsync(TKey id,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                   bool tracking = false);

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
        /// <returns> The entity. </returns>
        Task<TEntity> GetRequiredByIdAsync(TKey id,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                           bool tracking = false);

        /// <summary>
        ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns> The entity found or null. </returns>
        Task<TEntity> GetByIdAsync(TKey id,
                                   Func<IIncludable<TEntity>, IIncludable> includes,
                                   Expression<Func<TEntity, bool>> conditionExpression = null,
                                   Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                   bool tracking = false);


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
        /// <returns> The entity. </returns>
        Task<TEntity> GetRequiredByIdAsync(TKey id,
                                           Func<IIncludable<TEntity>, IIncludable> includes,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                           bool tracking = false);

        /// <summary>
        /// Groups entities with <paramref name="groupByPropertyName"/> and returns the key grouped and the number of items grouped with this key.
        /// </summary>
        /// <param name="groupByPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(string groupByPropertyName,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false);

        /// <summary>
        /// Groups entities with <paramref name="keySelector"/> and returns the key grouped and the number of items grouped with this key.
        /// </summary>
        /// <param name="keySelector"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(Expression<Func<TEntity, object>> keySelector,
                                                               Expression<Func<TEntity, bool>> conditionExpression = null,
                                                               bool tracking = false);

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
        /// <returns></returns>
        Task<List<TReturn>> GetAsGroupedAsync<TReturn>(IQueryable<TReturn> groupedClause,
                                                       Expression<Func<TReturn, bool>> conditionExpression = null);

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
        /// <returns></returns>
        Task<List<TReturn>> GetAsGroupedAsync<TReturn>(Func<IQueryable<TReturn>> groupedClause,
                                                       Expression<Func<TReturn, bool>> conditionExpression = null);

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
        /// <returns></returns>
        Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAsync<TReturn>(int requestedPageNumber,
                                                                                                    int countOfRequestedRecordsInPage,
                                                                                                    Func<IQueryable<TReturn>> groupedClause,
                                                                                                    Expression<Func<TReturn, bool>> conditionExpression = null);

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
        /// <returns></returns>
        Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAndOrderedAsync<TReturn>(int requestedPageNumber,
                                                                                                              int countOfRequestedRecordsInPage,
                                                                                                              string orderByPropertyName,
                                                                                                              bool orderByAscending,
                                                                                                              Func<IQueryable<TReturn>> groupedClause,
                                                                                                              Expression<Func<TReturn, bool>> conditionExpression = null);

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
        /// <returns></returns>
        Task<IEnumerable<TReturn>> GetAsGroupedAndOrderedAsync<TReturn>(string orderByPropertyName,
                                                                        bool orderByAscending,
                                                                        Func<IQueryable<TReturn>> groupedClause,
                                                                        Expression<Func<TReturn, bool>> conditionExpression = null);

        /// <summary>
        /// Get max value of entities.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync(Expression<Func<TEntity, bool>> conditionExpression = null,
                                  Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                  bool tracking = false);

        /// <summary>
        /// Get max value of entities. With includes.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                  Expression<Func<TEntity, bool>> conditionExpression = null,
                                  Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                  bool tracking = false);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxProperty"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<TEntity> GetMaxAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                             Expression<Func<TEntity, bool>> conditionExpression = null,
                                             Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                             bool tracking = false);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxProperty"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="projectionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<object> GetMaxAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                            Func<IIncludable<TEntity>, IIncludable> includes,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            Expression<Func<TEntity, TEntity>> projectionExpression = null,
                                            bool tracking = false);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<object> GetMaxOfPropertyAsync(string maxPropertyName,
                                           Expression<Func<TEntity, bool>> conditionExpression = null,
                                           bool tracking = false);

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxProperty"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<object> GetMaxOfPropertyAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty,
                                                      Expression<Func<TEntity, bool>> conditionExpression = null,
                                                      bool tracking = false);

        /// <summary>
        /// Gets sums of value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="sumProperty"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<decimal> GetSumOfPropertyAsync(Expression<Func<TEntity, decimal>> sumProperty,
                                            Expression<Func<TEntity, bool>> conditionExpression = null,
                                            bool tracking = false);

        /// <summary>
        /// Get count of entities.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> conditionExpression = null, bool tracking = false);

        #endregion

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(TKey id);

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null);

        /// <summary>
        ///  Adds single entity to database asynchronously. 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(TEntity entity);

        /// <summary>
        ///  Adds multiple entities to database asynchronously. 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        ///  Updates specified entity in database asynchronously.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Specific properties updates.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="projectionProperties"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] projectionProperties);

        /// <summary>
        /// Specific properties updates.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="projectionProperties"></param>
        /// <returns></returns>
        Task UpdateAsync(IEnumerable<TEntity> entities, params Expression<Func<TEntity, object>>[] projectionProperties);

        /// <summary>
        ///  Updates multiple entities in database asynchronously.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task UpdateAsync(IEnumerable<TEntity> entities);

        /// <summary>
        ///  Deletes single entity from database asynchronously.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        ///  Deletes multiple entity from database asynchronously.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task DeleteAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
        /// </summary>
        /// <param name="oldEntities"></param>
        /// <param name="newEntities"></param>
        /// <returns></returns>
        Task ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

        /// <summary>
        /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
        /// </summary>
        /// <param name="oldEntities"></param>
        /// <param name="newEntities"></param>
        /// <returns></returns>
        Task ReplaceOldsWithNewsInSeperateDatabaseProcessAsync(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities);

        /// <summary>
        /// Removes all entities from database.
        /// </summary>
        /// <returns></returns>
        Task RemoveAllAsync();
    }
}
