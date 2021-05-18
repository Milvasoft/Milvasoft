using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
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
        Task<PaginationDTO<StudentForMentorDTO>> GetStudentsForCurrentMentorAsync(PaginationParamsWithSpec<StudentSpec> paginationParams);

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
        /// <param name="studentDTO"></param>
        /// <returns></returns>
        Task AddStudentAsync(AddStudentDTO studentDTO);

        /// <summary>
        /// Updates single student which that equals <paramref name="Id"/> in repository by <paramref name="updateStudentDTO"/>'s properties by admin.
        /// </summary>
        /// <param name="updateStudentDTO">Student to be updated.</param>
        /// <param name="Id">Id of student to be updated.</param>
        /// <returns></returns>
        Task UpdateStudentByAdminAsync(UpdateStudentByAdminDTO updateStudentDTO, Guid Id);

        /// <summary>
        /// Updates single student which that equals <paramref name="Id"/> in repository by <paramref name="updateStudentDTO"/>'s properties by mentor.
        /// </summary>
        /// <param name="updateStudentDTO">Student to be updated.</param>
        /// <param name="Id">Id of student to be updated.</param>
        /// <returns></returns>
        Task UpdateStudentByMentorAsync(UpdateStudentByMentorDTO updateStudentDTO,Guid Id);

        /// <summary>
        /// Updates the information of the logged in student.
        /// </summary>
        /// <param name="studentDTO"></param>
        /// <returns></returns>
        Task UpdateCurrentStudentAsync(UpdateStudentDTO studentDTO);

        /// <summary>
        /// Delete students by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteStudentsAsync(List<Guid> ids);

        /// <summary>
        /// Brings instant user's profile information.
        /// </summary>
        /// <returns></returns>
        Task<StudentForMentorDTO> GetCurrentUserProfile();

        
    }
}
