using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Student service inteface.
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Get all students for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<StudentForAdminDTO>> GetStudentsForAdminAsync(PaginationParamsWithSpec<StudentSpec> paginationParams);

        /// <summary>
        /// Get all students for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<StudentForMentorDTO>> GetStudentsForMentorAsync(PaginationParamsWithSpec<StudentSpec> paginationParams);

        /// <summary>
        /// Fetches filtered student by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<StudentForAdminDTO> GetStudentForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered student by id for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<StudentForMentorDTO> GetStudentForMentorAsync(Guid id);

        /// <summary>
        /// Add student to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddStudentAsync(AddStudentDTO educationDTO);

        /// <summary>
        /// Updates student in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateStudentAsync(UpdateStudentDTO educationDTO);

        /// <summary>
        /// Delete students by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteStudentsAsync(List<Guid> ids);

    }
}
