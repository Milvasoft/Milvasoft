using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs;
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

        #region Student

        /// <summary>
        /// Brings homework suitable for the student's level.
        /// </summary>
        /// <returns></returns>
        Task<AssignmentForStudentDTO> GetAvaibleAssignmentForCurrentStudent();

        /// <summary>
        /// Get current assignment for logged student.
        /// </summary>
        /// <returns></returns>
        Task<AssignmentForStudentDTO> GetCurrentActiveAssignment();

        /// <summary>
        ///  The student takes the next assignment.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="newAssignment"></param>
        /// <returns></returns>
        Task TakeAssignment(Guid Id, AddStudentAssignmentDTO newAssignment);

        /// <summary>
        /// Allows the student to turn in the assignment.
        /// </summary>
        /// <param name="submitAssignment"></param>
        /// <returns></returns>
        Task<string> SubmitAssignment(SubmitAssignmentDTO submitAssignment);

        #endregion

        #region Mentor

        /// <summary>
        /// Brings the unapproved assignments of the students of the mentor logged in.
        /// </summary>
        /// <returns></returns>
        Task<List<StudentAssignmentDTO>> GetUnconfirmedAssignment();

        /// <summary>
        /// The mentor approves the homework request sent by the student.
        /// </summary>
        /// <param name="toBeUpdated"></param>
        /// <returns></returns>
        Task ConfirmAssignment(StudentAssignmentDTO toBeUpdated);

        #endregion
    }
}
