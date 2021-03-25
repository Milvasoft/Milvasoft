using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{

    /// <summary>
    /// Base services for all concrete services.
    /// </summary>
    /// <typeparam name="TDTO"></typeparam>
    public interface IBaseService<TDTO,TSpec>
    {
        /// <summary>
        /// Get all entities for student from database.
        /// </summary>
        /// <returns></returns>
        Task<List<TDTO>> GetEntitiesForStudent(TSpec spec);

        /// <summary>
        /// Get all entities for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<List<TDTO>> GetEntitiesForAdmin(TSpec spec);

        /// <summary>
        /// Get all entities for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<List<TDTO>> GetEntitiesForMentor(TSpec spec);

        /// <summary>
        /// Get one entity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForStudent(Guid id);

        /// <summary>
        /// Get one entity for admin by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForAdmin (Guid id);

        /// <summary>
        /// Get one entity for mentor by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForMentor(Guid id);

        /// <summary>
        /// Adds single entity to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddEntityAsync(TDTO educationDTO);

        /// <summary>
        /// Updates single entity in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateEntityAsync(TDTO educationDTO);

        /// <summary>
        /// Deletes single entity from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteEntityAsync(Guid id);

        /// <summary>
        /// Delete entities by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteEntities(List<Guid> ids);
    }
}
