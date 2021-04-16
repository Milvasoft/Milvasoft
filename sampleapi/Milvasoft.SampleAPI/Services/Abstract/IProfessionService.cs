using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.ProfessionDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Profession service interface.
    /// </summary>
    public interface IProfessionService 
    {
        /// <summary>
        /// Test method.
        /// </summary>
        /// <returns></returns>
        Task TestMethod();

        /// <summary>
        /// Get all profession for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<ProfessionDTO>> GetProfessionsForStudentAsync(PaginationParamsWithSpec<ProfessionSpec> paginationParams);

        /// <summary>
        /// Get all profession for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<ProfessionDTO>> GetProfessionsForAdminAsync(PaginationParamsWithSpec<ProfessionSpec> paginationParams);

        /// <summary>
        /// Get all profession for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<ProfessionDTO>> GetProfessionsForMentorAsync(PaginationParamsWithSpec<ProfessionSpec> paginationParams);

        /// <summary>
        /// Fetches filtered profession by id for student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProfessionDTO> GetProfessionForStudentAsync(Guid id);

        /// <summary>
        /// Fetches filtered profession by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProfessionDTO> GetProfessionForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered profession by id for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProfessionDTO> GetProfessionForMentorAsync(Guid id);

        /// <summary>
        /// Add profession to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddProfessionAsync(AddProfessionDTO educationDTO);

        /// <summary>
        /// Updates profession in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateProfessionAsync(UpdateProfessionDTO educationDTO);

        /// <summary>
        /// Delete profession by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteProfessionsAsync(List<Guid> ids);
    }
}
