using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Helpers;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Student operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1.0")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        /// <summary>
        /// Constructor of <c>StudentController</c>
        /// </summary>
        /// <param name="studentService"></param>
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Gets the all filtered students datas for log in mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Mentor")]
        public async Task<IActionResult> GetStudentsForMentor([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetStudentsForCurrentMentorAsync(paginationParams).ConfigureAwait(false);

            return students.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered students datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Admin")]
        public async Task<IActionResult> GetStudentsForAdmin([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetStudentsForAdminAsync(paginationParams).ConfigureAwait(false);

            return students.GetObjectResponse("Success");
        }

        /// <summary>
        /// Brings the information of the student who is logged in.
        /// </summary>
        /// <returns></returns>
        [HttpPatch("Student")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var currentStudent = await _studentService.GetCurrentUserProfile().ConfigureAwait(false);

            return currentStudent.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the all filtered student datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Admin/{id}")]
        public async Task<IActionResult> GetStudentForAdminbyId(Guid id)
        {
            var student = await _studentService.GetStudentForAdminAsync(id).ConfigureAwait(false);

            return student.GetObjectResponse("Success");
        }

        /// <summary>
        /// Gets the filtered student datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Mentor/{id}")]
        public async Task<IActionResult> GetStudentForMentorbyId(Guid id)
        {
            var student = await _studentService.GetStudentForAdminAsync(id).ConfigureAwait(false);

            return student.GetObjectResponse("Success");
        }

        /// <summary>
        /// Add <b><paramref name="addStudent"/></b> data to database.
        /// </summary>
        /// <param name="addStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("Student")]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentDTO addStudent)
        {
            return await _studentService.AddStudentAsync(addStudent).ConfigureAwait(false).GetObjectResponseAsync<AddStudentDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <returns></returns>
        [HttpPut("Student")]
        public async Task<IActionResult> UpdateCurrentStudent([FromBody] UpdateStudentDTO updateStudent)
        {
            return await _studentService.UpdateCurrentStudentAsync(updateStudent).ConfigureAwait(false).GetObjectResponseAsync<UpdateStudentDTO>("Success").ConfigureAwait(false);
        }
        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPut("StudentbyMentor")]
        public async Task<IActionResult> UpdateStudentbyMentor([FromBody] UpdateStudentByMentorDTO updateStudent, Guid Id)
        {
            return await _studentService.UpdateStudentByMentorAsync(updateStudent,Id).ConfigureAwait(false).GetObjectResponseAsync<UpdateStudentDTO>("Success").ConfigureAwait(false);
        }
        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPut("StudentbyAdmin")]
        public async Task<IActionResult> UpdateStudentbyAdmin([FromBody] UpdateStudentByAdminDTO updateStudent, Guid Id)
        {
            return await _studentService.UpdateStudentByAdminAsync(updateStudent,Id).ConfigureAwait(false).GetObjectResponseAsync<UpdateStudentDTO>("Success").ConfigureAwait(false);
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        // [Authorize(Roles = "Admin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteStudents([FromBody] List<Guid> ids)
        {
            return await _studentService.DeleteStudentsAsync(ids).ConfigureAwait(false).GetObjectResponseAsync<StudentDTO, Guid>(ids, "Success");
        }

    }

}