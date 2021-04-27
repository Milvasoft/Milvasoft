using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    ///  Base repository for concrete repositories. All Ops!yon repositories must be have this methods.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public abstract class BaseRepository<TEntity, TKey, TContext> : IBaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                                                                             where TKey : struct, IEquatable<TKey>
                                                                                                             where TContext : DbContext
    {
        //TODO EntityFrameworkQueryableExtensions methods will be added here.

        #region Protected Properties

        /// <summary>
        /// DbContext object.
        /// </summary>
        protected readonly TContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        #endregion

        #region Private Properties

        private bool _resetSoftDeleteState = true;
        private bool _softDeleteState = false;

        /// <summary>
        /// Determines whether soft deleted entities in the database are fetched from the database.
        /// <para><b>Default is false.</b></para>
        /// </summary>
        /// <param name="state"></param>
        public void SoftDeleteState(bool state) => _softDeleteState = state;

        /// <summary>
        /// Determines whether the default value of the variable that determines the status of deleted data in the database is assigned to the default value after database operation.
        /// </summary>
        /// <param name="state"></param>
        public void ResetSoftDeleteState(bool state) => _resetSoftDeleteState = state;

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
        /// Gets <b>entity => entity.IsDeleted == false</b> expression, if <typeparamref name="TEntity"/> is assignable from <see cref="FullAuditableEntity{TKey}"/>.
        /// </summary>
        /// <returns></returns>
        public virtual Expression<Func<TEntity, bool>> CreateIsDeletedFalseExpression()
        {
            var entityType = typeof(TEntity);
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType))
            {
                var parameter = Expression.Parameter(entityType, "entity");
                var filterExpression = Expression.Equal(Expression.Property(parameter, entityType.GetProperty(EntityPropertyNames.IsDeleted)), Expression.Constant(false, typeof(bool)));
                return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
            }
            else return null;
        }

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition. 
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetFirstOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.IncludeMultiple(includes).FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.SingleOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true with includes from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetSingleOrDefaultAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.IncludeMultiple(includes).SingleOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true)).ToListAsync().ConfigureAwait(false);

        /// <summary>
        ///  Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true)).IncludeMultiple(includes).ToListAsync().ConfigureAwait(false);

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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                                          int countOfRequestedRecordsInPage,
                                                                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            var repo = await _dbSet.Where(condition ?? (entity => true))
                                                      .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                      .Take(countOfRequestedRecordsInPage)
                                                      .ToListAsync().ConfigureAwait(false);

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                                          int countOfRequestedRecordsInPage,
                                                                                                                          Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            var repo = await _dbSet.Where(condition ?? (entity => true))
                                                      .IncludeMultiple(includes)
                                                      .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                      .Take(countOfRequestedRecordsInPage)
                                                      .ToListAsync().ConfigureAwait(false);

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
        }

        /// <summary>
        ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count and range.
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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                    int countOfRequestedRecordsInPage,
                                                                                                                                    string orderByPropertyName,
                                                                                                                                    bool orderByAscending,
                                                                                                                                    Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var entityType = typeof(TEntity);

            CheckProperty(orderByPropertyName, entityType);

            var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = await _dbSet.Where(condition ?? (entity => true))
                                                                        .OrderBy(predicate)
                                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                        .Take(countOfRequestedRecordsInPage)
                                                                        .ToListAsync().ConfigureAwait(false);
            else repo = await _dbSet.Where(condition ?? (entity => true))
                                                       .OrderByDescending(predicate)
                                                       .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                       .Take(countOfRequestedRecordsInPage)
                                                       .ToListAsync().ConfigureAwait(false);

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
        }

        /// <summary>
        ///  Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                    int countOfRequestedRecordsInPage,
                                                                                                                                    Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                                    string orderByPropertyName,
                                                                                                                                    bool orderByAscending,
                                                                                                                                    Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var entityType = typeof(TEntity);

            CheckProperty(orderByPropertyName, entityType);

            var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = (await _dbSet.Where(condition ?? (entity => true))
                                                                         .OrderBy(predicate)
                                                                         .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                         .Take(countOfRequestedRecordsInPage)
                                                                         .IncludeMultiple(includes)
                                                                         .ToListAsync().ConfigureAwait(false));
            else repo = (await _dbSet.Where(condition ?? (entity => true))
                                                        .OrderByDescending(predicate)
                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                        .Take(countOfRequestedRecordsInPage)
                                                        .IncludeMultiple(includes)
                                                        .ToListAsync().ConfigureAwait(false));

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                    int countOfRequestedRecordsInPage,
                                                                                                                                    Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                                    bool orderByAscending,
                                                                                                                                    Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = await _dbSet.Where(condition ?? (entity => true))
                                                                        .OrderBy(orderByKeySelector)
                                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                        .Take(countOfRequestedRecordsInPage)
                                                                        .ToListAsync().ConfigureAwait(false);
            else repo = await _dbSet.Where(condition ?? (entity => true))
                                                       .OrderByDescending(orderByKeySelector)
                                                       .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                       .Take(countOfRequestedRecordsInPage)
                                                       .ToListAsync().ConfigureAwait(false);

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount, int totalDataCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                                    int countOfRequestedRecordsInPage,
                                                                                                                                    Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                                    Expression<Func<TEntity, object>> orderByKeySelector,
                                                                                                                                    bool orderByAscending,
                                                                                                                                    Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = (await _dbSet.Where(condition ?? (entity => true))
                                                                         .OrderBy(orderByKeySelector)
                                                                         .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                         .Take(countOfRequestedRecordsInPage)
                                                                         .IncludeMultiple(includes)
                                                                         .ToListAsync().ConfigureAwait(false));
            else repo = (await _dbSet.Where(condition ?? (entity => true))
                                                        .OrderByDescending(orderByKeySelector)
                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                        .Take(countOfRequestedRecordsInPage)
                                                        .IncludeMultiple(includes)
                                                        .ToListAsync().ConfigureAwait(false));

            var estimatedCountOfPages = CalculatePageCountAndCompareWithRequested(totalDataCount, countOfRequestedRecordsInPage, requestedPageNumber);

            return (entities: repo, pageCount: estimatedCountOfPages, totalDataCount: totalDataCount);
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
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(string orderByPropertyName,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var entityType = typeof(TEntity);

            CheckProperty(orderByPropertyName, entityType);

            var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = await _dbSet.Where(condition ?? (entity => true))
                                                                        .OrderBy(predicate)
                                                                        .ToListAsync().ConfigureAwait(false);
            else repo = await _dbSet.Where(condition ?? (entity => true))
                                                       .OrderByDescending(predicate)
                                                       .ToListAsync().ConfigureAwait(false);

            return repo;
        }

        /// <summary>
        ///  Gets entities as ordered with <paramref name="orderByPropertyName"/>.
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
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                          string orderByPropertyName,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var entityType = typeof(TEntity);

            CheckProperty(orderByPropertyName, entityType);

            var predicate = CreateObjectPredicate(entityType, orderByPropertyName);

            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = (await _dbSet.Where(condition ?? (entity => true))
                                                                         .OrderBy(predicate)
                                                                         .IncludeMultiple(includes)
                                                                         .ToListAsync().ConfigureAwait(false));
            else repo = (await _dbSet.Where(condition ?? (entity => true))
                                                        .OrderByDescending(predicate)
                                                        .IncludeMultiple(includes)
                                                        .ToListAsync().ConfigureAwait(false));

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
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Expression<Func<TEntity, object>> orderByKeySelector,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = await _dbSet.Where(condition ?? (entity => true))
                                                                        .OrderBy(orderByKeySelector)
                                                                        .ToListAsync().ConfigureAwait(false);
            else repo = await _dbSet.Where(condition ?? (entity => true))
                                                       .OrderByDescending(orderByKeySelector)
                                                       .ToListAsync().ConfigureAwait(false);

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
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAsOrderedAsync(Func<IIncludable<TEntity>, IIncludable> includes,
                                                                          Expression<Func<TEntity, object>> orderByKeySelector,
                                                                          bool orderByAscending,
                                                                          Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var condition = CreateConditionExpression(conditionExpression);

            var totalDataCount = await GetCountAsync(conditionExpression).ConfigureAwait(false);

            List<TEntity> repo;

            if (orderByAscending) repo = (await _dbSet.Where(condition ?? (entity => true))
                                                                         .OrderBy(orderByKeySelector)
                                                                         .IncludeMultiple(includes)
                                                                         .ToListAsync().ConfigureAwait(false));
            else repo = (await _dbSet.Where(condition ?? (entity => true))
                                                        .OrderByDescending(orderByKeySelector)
                                                        .IncludeMultiple(includes)
                                                        .ToListAsync().ConfigureAwait(false));

            return repo;
        }

        #endregion


        /// <summary>
        /// Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="conditionExpression"></param>
        /// <returns> The entity found or null. </returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

            return await _dbSet.SingleOrDefaultAsync(mainCondition).ConfigureAwait(false);
        }

        /// <summary>
        ///  Returns one entity by entity Id from database asynchronously.
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
        /// 
        /// <param name="id"></param>
        /// <param name="conditionExpression"></param>
        /// <returns> The entity. </returns>
        public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

            return (await _dbSet.SingleAsync(mainCondition).ConfigureAwait(false));
        }

        /// <summary>
        ///  Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns> The entity found or null. </returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id,
                                                        Func<IIncludable<TEntity>, IIncludable> includes,
                                                        Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

            return await _dbSet.IncludeMultiple(includes).SingleOrDefaultAsync(mainCondition).ConfigureAwait(false);
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
        /// <returns> The entity. </returns>
        public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id,
                                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                                Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var mainCondition = CreateKeyEqualityExpression(id, conditionExpression);

            return await _dbSet.IncludeMultiple(includes).SingleAsync(mainCondition).ConfigureAwait(false);
        }

        /// <summary>
        ///  Adds single entity to database asynchronously. 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task AddAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Adds multiple entities to database asynchronously. 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Updates specified entity in database asynchronously.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            InitalizeEdit(entity);
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Updates multiple entities in database asynchronously.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(IEnumerable<TEntity> entities)
        {
            InitalizeEdit(entities);
            _dbSet.UpdateRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Deletes single entity from database asynchronously.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            InitalizeEdit(entity);
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Deletes multiple entity from database asynchronously.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(IEnumerable<TEntity> entities)
        {
            InitalizeEdit(entities);
            _dbContext.RemoveRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Groups entities with <paramref name="groupByPropertyName"/> and returns the key grouped and the number of items grouped with this key.
        /// </summary>
        /// <param name="groupByPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(string groupByPropertyName, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var entityType = typeof(TEntity);

            CheckProperty(groupByPropertyName, entityType);

            var predicate = CreateObjectPredicate(entityType, groupByPropertyName);

            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                   .GroupBy(predicate)
                                                   .Select(b => Tuple.Create(b.Key, b.Count()))
                                                   .ToListAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Groups entities with <paramref name="keySelector"/> and returns the key grouped and the number of items grouped with this key.
        /// </summary>
        /// <param name="keySelector"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<List<Tuple<object, int>>> GetGroupedAndCountAsync(Expression<Func<TEntity, object>> keySelector, Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                              .GroupBy(keySelector)
                                              .Select(b => Tuple.Create(b.Key, b.Count()))
                                              .ToListAsync().ConfigureAwait(false);

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
        public virtual async Task<List<TReturn>> GetAsGroupedAsync<TReturn>(IQueryable<TReturn> groupedClause, Expression<Func<TReturn, bool>> conditionExpression = null)
            => await groupedClause.Where(conditionExpression ?? (entity => true)).ToListAsync().ConfigureAwait(false);

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
        public virtual async Task<List<TReturn>> GetAsGroupedAsync<TReturn>(Func<IQueryable<TReturn>> groupedClause, Expression<Func<TReturn, bool>> conditionExpression = null)
            => await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).ToListAsync().ConfigureAwait(false);

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
        public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAsync<TReturn>(int requestedPageNumber,
                                                                                                                         int countOfRequestedRecordsInPage,
                                                                                                                         Func<IQueryable<TReturn>> groupedClause,
                                                                                                                         Expression<Func<TReturn, bool>> conditionExpression = null)
        {
            ValidatePaginationParameters(requestedPageNumber, countOfRequestedRecordsInPage);

            var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync().ConfigureAwait(false);

            var repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage).Take(countOfRequestedRecordsInPage).ToListAsync().ConfigureAwait(false);

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
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TReturn> entities, int pageCount)> GetAsGroupedAndPaginatedAndOrderedAsync<TReturn>(int requestedPageNumber,
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

            var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync().ConfigureAwait(false);

            List<TReturn> repo;

            if (orderByAscending) repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                                     .OrderBy(predicate)
                                                                     .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                                     .Take(countOfRequestedRecordsInPage)
                                                                     .ToListAsync().ConfigureAwait(false);
            else repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                    .OrderByDescending(predicate)
                                                    .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                    .Take(countOfRequestedRecordsInPage)
                                                    .ToListAsync().ConfigureAwait(false);

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
        /// <returns></returns>
        public virtual async Task<IEnumerable<TReturn>> GetAsGroupedAnOrderedAsync<TReturn>(string orderByPropertyName,
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

            var totalDataCount = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true)).CountAsync().ConfigureAwait(false);

            List<TReturn> repo;

            if (orderByAscending)
                repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                       .OrderBy(predicate)
                                                           .ToListAsync().ConfigureAwait(false);
            else
                repo = await groupedClause.Invoke().Where(conditionExpression ?? (entity => true))
                                                       .OrderByDescending(predicate)
                                                            .ToListAsync().ConfigureAwait(false);
            return repo;
        }

        /// <summary>
        /// Get max value of entities.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetMaxAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                   .MaxAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Get max value of entities. With includes.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetMaxAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                   .IncludeMultiple(includes)
                                                   .MaxAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxPropertyName"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<object> GetMaxOfPropertyAsync(string maxPropertyName, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var entityType = typeof(TEntity);

            if (!CommonHelper.PropertyExists<TEntity>(maxPropertyName))
                throw new MilvaDeveloperException($"Type of {entityType}'s properties doesn't contain '{maxPropertyName}'.");

            var parameterExpression = Expression.Parameter(entityType, "i");

            Expression orderByProperty = Expression.Property(parameterExpression, maxPropertyName);

            var predicate = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);

            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                   .MaxAsync(predicate).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets max value of <typeparamref name="TEntity"/>'s property in entities.
        /// </summary>
        /// <param name="maxProperty"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<object> GetMaxOfPropertyAsync<TProperty>(Expression<Func<TEntity, TProperty>> maxProperty, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                   .MaxAsync(maxProperty).ConfigureAwait(false));
        }

        /// <summary>
        /// Get count of entities.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            return (await _dbSet.Where(CreateConditionExpression(conditionExpression) ?? (entity => true))
                                                          .CountAsync().ConfigureAwait(false));
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
        /// Replaces existing entities(<paramref name="oldEntities"/>) with new entities(<paramref name="newEntities"/>).
        /// </summary>
        /// <param name="oldEntities"></param>
        /// <param name="newEntities"></param>
        /// <returns></returns>
        public virtual async Task ReplaceOldsWithNewsAsync(IEnumerable<TEntity> oldEntities, IEnumerable<TEntity> newEntities)
        {
            if (!oldEntities.IsNullOrEmpty())
            {
                InitalizeEdit(oldEntities);
                _dbContext.RemoveRange(oldEntities);
            }
            if (!newEntities.IsNullOrEmpty())
            {
                _dbSet.AddRange(newEntities);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes all entities from database.
        /// </summary>
        /// <returns></returns>
        public virtual async Task RemoveAllAsync()
        {
            var entities = _dbSet.AsEnumerable();
            InitalizeEdit(entities);
            _dbContext.RemoveRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists.
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
            => await _dbSet.FirstOrDefaultAsync(CreateConditionExpression(conditionExpression) ?? (entity => true)).ConfigureAwait(false) != null;

        /// <summary>
        /// Returns whether or not the entity that satisfies this condition exists. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> ExistsAsync(TKey id)
        {
            var mainCondition = CreateKeyEqualityExpression(id);

            return await _dbSet.SingleOrDefaultAsync(mainCondition).ConfigureAwait(false) != null;
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

            return await _dbSet.IncludeMultiple(includes).SingleOrDefaultAsync(mainCondition).ConfigureAwait(false) != null;
        }

        #region Private Helper Methods

        protected Expression<Func<TEntity, bool>> CreateKeyEqualityExpression(TKey key, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            Expression<Func<TEntity, bool>> idCondition = i => i.Id.Equals(key);
            var mainCondition = idCondition.Append(CreateIsDeletedFalseExpression(), ExpressionType.AndAlso);
            return mainCondition.Append(conditionExpression, ExpressionType.AndAlso);
        }

        protected Expression<Func<TEntity, object>> CreateObjectPredicate(Type entityType, string propertyName)
        {
            var parameterExpression = Expression.Parameter(entityType, "i");

            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);

            return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(orderByProperty, typeof(object)), parameterExpression);
        }

        protected static void CheckProperty(string propertyName, Type entityType)
        {
            if (!CommonHelper.PropertyExists<TEntity>(propertyName))
                throw new MilvaDeveloperException($"Type of {entityType.Name}'s properties doesn't contain '{propertyName}'.");
        }

        protected static int CalculatePageCountAndCompareWithRequested(int totalDataCount, int countOfRequestedRecordsInPage, int requestedPageNumber)
        {
            var actualPageCount = (Convert.ToDouble(totalDataCount) / Convert.ToDouble(countOfRequestedRecordsInPage));

            var estimatedCountOfPages = Convert.ToInt32(Math.Ceiling(actualPageCount));

            if (estimatedCountOfPages != 0 && requestedPageNumber > estimatedCountOfPages)
                throw new MilvaUserFriendlyException(MilvaException.WrongPaginationParams)
                { ExceptionObject = estimatedCountOfPages };

            return estimatedCountOfPages;
        }

        protected static void ValidatePaginationParameters(int requestedPageNumber, int countOfRequestedRecordsInPage)
        {
            if (requestedPageNumber <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedPageNumber);

            if (countOfRequestedRecordsInPage <= 0) throw new MilvaUserFriendlyException(MilvaException.WrongRequestedItemCount);
        }

        protected Expression<Func<TEntity, bool>> CreateConditionExpression(Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            Expression<Func<TEntity, bool>> mainExpression;

            //Step in when GetSoftDeletedEntities is false
            if (!_softDeleteState)
            {
                var softDeleteExpression = CreateIsDeletedFalseExpression();
                mainExpression = softDeleteExpression.Append(conditionExpression, ExpressionType.AndAlso);
            }
            else
            {
                mainExpression = conditionExpression;
                if (_resetSoftDeleteState) _softDeleteState = false;
            }
            return mainExpression;
        }

        /// <summary>
        /// For configure entity state. Change tracking.
        /// </summary>
        /// <param name="entity"></param>
        protected void InitalizeEdit(TEntity entity)
        {
            var local = _dbSet.Local.FirstOrDefault(entry => entry.Id.Equals(entity.Id));
            if (local != null)
            {
                _dbContext.Entry(local).State = EntityState.Detached;
            }
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// For configure entity state. Change tracking.
        /// </summary>
        /// <param name="entities"></param>
        protected void InitalizeEdit(IEnumerable<TEntity> entities)
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

        #endregion
    }
}
