using Milvasoft.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{

    /// <summary>
    /// Base services for all concrete services.
    /// </summary>
    /// <typeparam name="TDTO"></typeparam>
    public interface IBaseService<TDTO, TSpec, TAddDTO, TUpdateDTO>
    {
        /// <summary>
        /// Get all entities for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TDTO>> GetEntitiesForStudentAsync(int pageIndex,
                                                    int requestedItemCount,
                                                    string orderByProperty,
                                                    bool orderByAscending,
                                                    TSpec spec);

        /// <summary>
        /// Get all entities for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TDTO>> GetEntitiesForAdminAsync(int pageIndex,
                                                  int requestedItemCount,
                                                  string orderByProperty,
                                                  bool orderByAscending,
                                                  TSpec spec);

        /// <summary>
        /// Get all entities for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TDTO>> GetEntitiesForMentorAsync(int pageIndex,
                                                   int requestedItemCount,
                                                   string orderByProperty,
                                                   bool orderByAscending,
                                                   TSpec spec);

        /// <summary>
        /// Get one entity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForStudentAsync(Guid id);

        /// <summary>
        /// Get one entity for admin by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForAdminAsync(Guid id);

        /// <summary>
        /// Get one entity for mentor by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TDTO> GetEntityForMentorAsync(Guid id);

        /// <summary>
        /// Adds single entity to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddEntityAsync(TAddDTO educationDTO);

        /// <summary>
        /// Updates single entity in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateEntityAsync(TUpdateDTO educationDTO);

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
        Task DeleteEntitiesAsync(List<Guid> ids);
    }
}
