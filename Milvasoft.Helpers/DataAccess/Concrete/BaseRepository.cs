﻿using Microsoft.EntityFrameworkCore;
using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// <para> Base repository for concrete repositories. All Ops!yon repositories must be have this methods. </para> 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public abstract class BaseRepository<TEntity, TKey, TContext> : IBaseRepository<TEntity, TKey, TContext> where TEntity : class, IBaseEntity<TKey>
                                                                                                             where TKey : IEquatable<TKey>
                                                                                                             where TContext : DbContext
    {
        /// <summary>
        /// DbContext object.
        /// </summary>
        protected TContext _dbContext;

        /// <summary>
        /// Constructor of BaseRepository for <paramref name="dbContext"/> injection.
        /// </summary>
        /// <param name="dbContext"></param>
        public BaseRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// <para> Returns all entities which IsDeleted condition is true from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var result = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// <para> Returns all entities which IsDeleted condition is true with specified includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<IIncludable<TEntity>, IIncludable> includes, Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            return await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).IncludeMultiple(includes).ToListAsync().ConfigureAwait(false);
        }

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
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                      int countOfRequestedRecordsInPage,
                                                                                                      Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var dataCount = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).CountAsync().ConfigureAwait(false);

            var repo = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage).Take(countOfRequestedRecordsInPage).ToListAsync().ConfigureAwait(false);

            var actualPageCount = (Convert.ToDouble(dataCount) / Convert.ToDouble(countOfRequestedRecordsInPage));

            int estimatedCountOfRanges = Convert.ToInt32(Math.Ceiling(actualPageCount));

            if (requestedPageNumber > estimatedCountOfRanges)
                throw new ArgumentOutOfRangeException($"Requested page count is more than actual page count. Maximum page count must be {actualPageCount}.");

            return (entities: repo, pageCount: estimatedCountOfRanges);
        }

        /// <summary>
        /// <para> Creates asynchronously a shallow copy of a range of entity's which IsDeleted property is true, in the source List of TEntity with requested count,range and includes.
        ///       If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException"> Throwns when <paramref name="requestedPageNumber"/> more than actual page number. </exception>
        ///
        /// <param name="requestedPageNumber"></param>
        /// <param name="countOfRequestedRecordsInPage"></param>
        /// <param name="includes"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAsync(int requestedPageNumber,
                                                                                                      int countOfRequestedRecordsInPage,
                                                                                                      Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                      Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            var dataCount = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).CountAsync().ConfigureAwait(false);
            var repo = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).IncludeMultiple(includes).Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage).Take(countOfRequestedRecordsInPage).ToListAsync().ConfigureAwait(false);

            var actualPageCount = (Convert.ToDouble(dataCount) / Convert.ToDouble(countOfRequestedRecordsInPage));

            int estimatedCountOfRanges = Convert.ToInt32(Math.Ceiling(actualPageCount));

            if (requestedPageNumber > estimatedCountOfRanges)
                throw new ArgumentOutOfRangeException($"Requested page count is more than actual page count. Maximum page count must be {actualPageCount}.");

            return (entities: repo, pageCount: estimatedCountOfRanges);
        }

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
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                string orderByPropertyName,
                                                                                                                bool orderByAscending,
                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            List<TEntity> repo;


            var entityType = typeof(TEntity);

            if (!CommonHelper.PropertyExists<TEntity>(orderByPropertyName))
                throw new ArgumentException($"Type of {entityType}'s properties doesn't contain '{orderByPropertyName}'.");

            ParameterExpression paramterExpression = Expression.Parameter(entityType, "i");
            Expression orderByProperty = Expression.Property(paramterExpression, orderByPropertyName);

            Expression<Func<TEntity, object>> predicate = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(Expression.Property(paramterExpression, orderByPropertyName), typeof(object)), paramterExpression);

            var dataCount = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).CountAsync();

            if (orderByAscending)
                repo = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true))
                                                       .OrderBy(predicate)
                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                            .Take(countOfRequestedRecordsInPage)
                                                                    .ToListAsync().ConfigureAwait(false);
            else
                repo = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true))
                                                       .OrderByDescending(predicate)
                                                        .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                            .Take(countOfRequestedRecordsInPage)
                                                                    .ToListAsync().ConfigureAwait(false);

            var actualPageCount = (Convert.ToDouble(dataCount) / Convert.ToDouble(countOfRequestedRecordsInPage));

            int estimatedCountOfRanges = Convert.ToInt32(Math.Ceiling(actualPageCount));

            if (requestedPageNumber > estimatedCountOfRanges)
                throw new ArgumentOutOfRangeException($"Requested page count is more than actual page count. Maximum page count must be {actualPageCount}.");

            return (entities: repo, pageCount: estimatedCountOfRanges);
        }

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
        public virtual async Task<(IEnumerable<TEntity> entities, int pageCount)> GetAsPaginatedAndOrderedAsync(int requestedPageNumber,
                                                                                                                int countOfRequestedRecordsInPage,
                                                                                                                Func<IIncludable<TEntity>, IIncludable> includes,
                                                                                                                string orderByPropertyName,
                                                                                                                bool orderByAscending,
                                                                                                                Expression<Func<TEntity, bool>> conditionExpression = null)
        {
            List<TEntity> repo;

            var entityType = typeof(TEntity);

            if (!CommonHelper.PropertyExists<TEntity>(orderByPropertyName))
                throw new ArgumentException($"Type of {entityType}'s properties doesn't contain '{orderByPropertyName}'.");


            ParameterExpression paramterExpression = Expression.Parameter(entityType, "i");
            Expression orderByProperty = Expression.Property(paramterExpression, orderByPropertyName);

            Expression<Func<TEntity, object>> predicate = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(Expression.Property(paramterExpression, orderByPropertyName), typeof(object)), paramterExpression);

            var dataCount = await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true)).CountAsync();

            if (orderByAscending)
                repo = (await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true))
                                                        .OrderBy(predicate)
                                                          .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                            .Take(countOfRequestedRecordsInPage)
                                                                  .IncludeMultiple(includes)
                                                                      .ToListAsync().ConfigureAwait(false));
            else
                repo = (await _dbContext.Set<TEntity>().Where(conditionExpression ?? (entity => true))
                                                         .OrderByDescending(predicate)
                                                          .Skip((requestedPageNumber - 1) * countOfRequestedRecordsInPage)
                                                            .Take(countOfRequestedRecordsInPage)
                                                                  .IncludeMultiple(includes)
                                                                      .ToListAsync().ConfigureAwait(false));

            var actualPageCount = (Convert.ToDouble(dataCount) / Convert.ToDouble(countOfRequestedRecordsInPage));

            int estimatedCountOfRanges = Convert.ToInt32(Math.Ceiling(actualPageCount));

            if (requestedPageNumber > estimatedCountOfRanges)
                throw new ArgumentOutOfRangeException("Requested page count is more than actual page count.");

            return (entities: repo, pageCount: estimatedCountOfRanges);
        }


        /// <summary>
        ///<para> Returns one entity by entity Id from database asynchronously.</para> 
        /// </summary>
        /// <param name="id"></param>
        /// <returns> The entity found or null. </returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// <para> Returns one entity by entity Id from database asynchronously. </para> 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
        /// 
        /// <param name="id"></param>
        /// <returns> The entity. </returns>
        public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id)
        {
            var entity = (await _dbContext.Set<TEntity>().FindAsync(id).ConfigureAwait(false)) ?? throw new ArgumentNullException();
            return entity;
        }

        /// <summary>
        /// <para> Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns> The entity found or null. </returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes)
        {
            var entity = await _dbContext.Set<TEntity>().Where(entity => entity.Id.Equals(id)).IncludeMultiple(includes).FirstOrDefaultAsync().ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// <para> Returns one entity which IsDeleted condition is true by entity Id with includes from database asynchronously. If the condition is requested, it also provides that condition.</para> 
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when no entity found. </exception>
        /// 
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns> The entity. </returns>
        public virtual async Task<TEntity> GetRequiredByIdAsync(TKey id, Func<IIncludable<TEntity>, IIncludable> includes)
        {
            var entity = await _dbContext.Set<TEntity>().Where(entity => entity.Id.Equals(id)).IncludeMultiple(includes).FirstAsync().ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// <para> Adds single entity to database asynchronously.</para> 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task AddAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para> Adds multiple entities to database asynchronously.</para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para> Updates specified entity in database asynchronously.</para> 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            InitalizeEdit(entity);
            _dbContext.Set<TEntity>().Update(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para> Updates multiple entities in database asynchronously.</para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(IEnumerable<TEntity> entities)
        {
            InitalizeEdit(entities);
            _dbContext.Set<TEntity>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para> Deletes single entity from database asynchronously..</para> 
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
        /// <para> Deletes multiple entity from database asynchronously. </para> 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(IEnumerable<TEntity> entities)
        {
            InitalizeEdit(entities);
            _dbContext.RemoveRange(entities);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }


        #region Private Helper Methods
        private void InitalizeEdit(TEntity entity)
        {
            var localEntity = _dbContext.Set<TEntity>().Local.FirstOrDefault(e => e.Id.Equals(entity.Id));
            if (localEntity == null)
                return;
            _dbContext.Entry(localEntity).State = EntityState.Detached;
        }

        private void InitalizeEdit(IEnumerable<TEntity> entities)
        {
            var localEntities = _dbContext.Set<TEntity>().Local.Where(e => entities.Any(en => en.Id.Equals(e.Id)));
            if (!localEntities?.Any() ?? false)
                return;
            foreach (var entity in localEntities)
                _dbContext.Entry(entity).State = EntityState.Detached;
        }

        #endregion
    }
}
