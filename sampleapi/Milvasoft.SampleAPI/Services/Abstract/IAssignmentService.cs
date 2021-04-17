using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Assignment service interface.
    /// </summary>
    public interface IAssignmentService
    {
        /// <summary>
        /// Get all assignment for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AssignmentForStudentDTO>> GetAssignmentForStudentAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams);

        /// <summary>
        /// Get all assignment for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AssignmentForAdminDTO>> GetAssignmentForAdminAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams);

        /// <summary>
        /// Get all assignment for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<AssignmentForMentorDTO>> GetAssignmentForMentorAsync(PaginationParamsWithSpec<AssignmentSpec> paginationParams);

        /// <summary>
        /// Fetches filtered assignment by id for student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AssignmentForStudentDTO> GetAssignmentForStudentAsync(Guid id);

        /// <summary>
        /// Fetches filtered assignment by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AssignmentForAdminDTO> GetAssignmentForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered assignment by id for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AssignmentForMentorDTO> GetAssignmentForMentorAsync(Guid id);

        /// <summary>
        /// Add assignment to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddAssignmentAsync(AddAssignmentDTO educationDTO);

        /// <summary>
        /// Updates assignment in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateAssignmentAsync(UpdateAssignmentDTO educationDTO);

        /// <summary>
        /// Delete assignment by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteAssignmentAsync(List<Guid> ids);
    }
}
