using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
using Milvasoft.SampleAPI.DTOs.StudentAssignmentDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided assignment operations.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    [Route("sampleapi/[controller]")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assigmentService;

        /// <summary>
        /// Constructor of <c>AssignmentConroller</c>.
        /// </summary>
        /// <param name="assignmentService"></param>
        public AssignmentController(IAssignmentService assignmentService)
        {
            _assigmentService = assignmentService;
        }

        /// <summary>
        /// Gets the all filtered assignments datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetAssignmentsForMentor([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForMentorAsync(paginationParams).ConfigureAwait(false);
            return assignments.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered assignments datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetAssignmentsForAdmn([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForAdminAsync(paginationParams).ConfigureAwait(false);
            return assignments.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered assignments datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student")]
        public async Task<IActionResult> GetAssignmentsForStudent([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForStudentAsync(paginationParams).ConfigureAwait(false);
            return assignments.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered assignment datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetAssignmentForMentorbyId(Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForMentorAsync(id).ConfigureAwait(false);

            return assignment.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered assignment datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetAssignmentForAdminbyId(Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForAdminAsync(id).ConfigureAwait(false);

            return assignment.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered assignment datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student/{id}")]
        public async Task<IActionResult> GetAssignmentForStudentbyId(Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForStudentAsync(id).ConfigureAwait(false);

            return assignment.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addAssignment"/></b> data to database.
        /// </summary>
        /// <param name="addAssignment"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("Assignment")]
        public async Task<IActionResult> AddAssignment([FromBody] AddAssignmentDTO addAssignment)
        {
            return await _assigmentService.AddAssignmentAsync(addAssignment).ConfigureAwait(false).GetObjectResponseAsync<AddAssignmentDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Update <paramref name="updateAssignment"/> data.
        /// </summary>
        /// <param name="updateAssignment"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("Assignment")]
        public async Task<IActionResult> UpdateAssignment([FromBody] UpdateAssignmentDTO updateAssignment)
        {
            return await _assigmentService.UpdateAssignmentAsync(updateAssignment).ConfigureAwait(false).GetObjectResponseAsync<UpdateAssignmentDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Delete assignment data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAssignment([FromBody] List<Guid> ids)
        {
            return await _assigmentService.DeleteAssignmentAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<AssignmentDTO, Guid>(ids, "Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Get avaible assignment for log in user.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("GetAssignment")]
        public async Task<IActionResult> GetAssignmentForCurrentUser()
        {
            var assignment = await _assigmentService.GetAvaibleAssignmentForCurrentStudent().ConfigureAwait(false);

            return assignment.GetObjectResponse("Success");
        }

        /// <summary>
        /// The student takes the next assignment.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="newAssignment"></param>
        /// <returns></returns>
        [HttpPost("TakeAssignment")]
        public async Task<IActionResult> TakeAssigment(Guid Id,[FromBody] AddStudentAssignmentDTO newAssignment)
        {
            return await _assigmentService.TakeAssignment(Id, newAssignment).ConfigureAwait(false).GetObjectResponseAsync<AddStudentAssignmentDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Brings the unapproved assignments of the students of the mentor logged in.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("UnconfirmedAssignements")]
        public async Task<IActionResult> GetUnconfirmedAssignment()
        {
            var assignment = await _assigmentService.GetUnconfirmedAssignment().ConfigureAwait(false);

            return assignment.GetObjectResponse("Success");
        }

        [HttpPost("SubmitAssignment")]
        public async Task<IActionResult> SubmitAssignment([FromBody] SubmitAssignmentDTO submitAssignment)
        {
            var path = await _assigmentService.SubmitAssignment(submitAssignment).ConfigureAwait(false);
            return Ok(path);
        }
    }
}
