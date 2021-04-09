using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{

    /// <summary>
    /// Base services for all concrete services.
    /// </summary>
    /// <typeparam name="TAddDTO"> DTO for add operations.</typeparam>
    /// <typeparam name="TEntityForAdminDTO"> DTO that returns when data is fetched for admin.</typeparam>
    /// <typeparam name="TEntityForMentorDTO">Get entity or entities for mentor.</typeparam>
    /// <typeparam name="TEntityForStudentDTO">Get entity or entities for student.</typeparam>
    /// <typeparam name="TSpec">Spec for filter operations.</typeparam>
    /// <typeparam name="TUpdateDTO">DTO for update operations.</typeparam>
    public interface IBaseService<TSpec, TAddDTO, TUpdateDTO, TEntityForStudentDTO, TEntityForMentorDTO, TEntityForAdminDTO>
    {
        /// <summary>
        /// Get all entities for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TEntityForStudentDTO>> GetEntitiesForStudentAsync(PaginationParamsWithSpec<TSpec> paginationParams);

        /// <summary>
        /// Get all entities for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TEntityForAdminDTO>> GetEntitiesForAdminAsync(PaginationParamsWithSpec<TSpec> paginationParams);

        /// <summary>
        /// Get all entities for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<TEntityForMentorDTO>> GetEntitiesForMentorAsync(PaginationParamsWithSpec<TSpec> paginationParams);

        /// <summary>
        /// Get one entity by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntityForStudentDTO> GetEntityForStudentAsync(Guid id);

        /// <summary>
        /// Get one entity for admin by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntityForAdminDTO> GetEntityForAdminAsync(Guid id);

        /// <summary>
        /// Get one entity for mentor by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntityForMentorDTO> GetEntityForMentorAsync(Guid id);

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
