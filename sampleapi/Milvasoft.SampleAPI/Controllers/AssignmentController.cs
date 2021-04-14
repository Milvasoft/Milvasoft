using Microsoft.AspNetCore.Mvc;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.AssignmentDTOs;
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
        [HttpPatch("Assignment/Mentor")]
        public async Task<IActionResult> GetAssignmentsForMentor([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForMentorAsync(paginationParams).ConfigureAwait(false);
            return Ok(assignments);
        }

        /// <summary>
        /// Gets the all filtered assignments datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Assignment/Admin")]
        public async Task<IActionResult> GetAssignmentsForAdmn([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForAdminAsync(paginationParams).ConfigureAwait(false);
            return Ok(assignments);
        }

        /// <summary>
        /// Gets the all filtered assignments datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Assignment/Student")]
        public async Task<IActionResult> GetAssignmentsForStudent([FromBody] PaginationParamsWithSpec<AssignmentSpec> paginationParams)
        {
            var assignments = await _assigmentService.GetAssignmentForStudentAsync(paginationParams).ConfigureAwait(false);
            return Ok(assignments);
        }

        /// <summary>
        /// Gets the all filtered assignment datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Assignment/Mentor/{id}")]
        public async Task<IActionResult> GetAssignmentForMentorbyId([FromBody] Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForMentorAsync(id).ConfigureAwait(false);

            return Ok(assignment);
        }

        /// <summary>
        /// Gets the filtered assignment datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Assignment/Admin/{id}")]
        public async Task<IActionResult> GetAssignmentForAdminbyId([FromBody] Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForAdminAsync(id).ConfigureAwait(false);

            return Ok(assignment);
        }

        /// <summary>
        /// Gets the filtered assignment datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Assignment/Student/{id}")]
        public async Task<IActionResult> GetAssignmentForStudentbyId([FromBody] Guid id)
        {
            var assignment = await _assigmentService.GetAssignmentForStudentAsync(id).ConfigureAwait(false);

            return Ok(assignment);
        }

        /// <summary>
        /// Add <b><paramref name="addAssignment"/></b> data to database.
        /// </summary>
        /// <param name="addAssignment"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("AddAssignment")]
        public async Task<IActionResult> AddAssignment([FromBody] AddAssignmentDTO addAssignment)
        {
            await _assigmentService.AddAssignmentAsync(addAssignment).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateAssignment"/> data.
        /// </summary>
        /// <param name="updateAssignment"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateAssignment")]
        public async Task<IActionResult> UpdateAssignment([FromBody] UpdateAssignmentDTO updateAssignment)
        {
            await _assigmentService.UpdateAssignmentAsync(updateAssignment).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete assignment data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Assignment/Deletes/{ids}")]
        public async Task<IActionResult> DeleteAssignment(List<Guid> ids)
        {
            await _assigmentService.DeleteAssignmentAsync(ids).ConfigureAwait(false);
            return Ok();
        }
    }
}
