using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Data.Abstract
{
    public interface IGenericRepository<TEntity>
    {
        /// <summary>
        /// Gets all entities from database.
        /// If <paramref name="include"/> is provided, returns with joined tables.
        /// </summary>
        /// <param name="include"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetEntitiesAsync(Expression<Func<TEntity, object>> include = null);

        /// <summary>
        /// Gets single entity by <paramref name="id"/>.
        /// If <paramref name="include"/> is provided, returns with joined tables.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        Task<TEntity> GetEntityAsync(Guid id, Expression<Func<TEntity, object>> include = null);

        /// <summary>
        /// Addd <paramref name="entity"/> to database.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<Guid> AddAsync(TEntity entity);

        /// <summary>
        /// Updates requested <paramref name="entity"/> in database.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes requested <paramref name="entity"/> from database.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task DeleteAsync(TEntity entity);
    }
}
