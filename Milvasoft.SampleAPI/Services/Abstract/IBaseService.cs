using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    public interface IBaseService<TDTO>
    {
        /// <summary>
        /// Get all entities from database.
        /// </summary>
        /// <returns></returns>
        Task<List<TDTO>> GetEntitiesAsync();

        /// <summary>
        /// Get one entity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityAsync(Guid id);

        /// <summary>
        /// Adds single entity to database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Guid> AddEntityAsync(TDTO todoDTO);

        /// <summary>
        /// Updates single entity in database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task UpdateEntityAsync(TDTO todoDTO);

        /// <summary>
        /// Deletes single entity from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteEntityAsync(Guid id);

        /// <summary>
        /// Sended favorites as mark.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isFavorite"></param>
        /// <returns></returns>
        Task MarkAsIsFavoriteAsync(Guid id, bool isFavorite);
    }
}
