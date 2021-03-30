﻿using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
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
        /// Gets the all filtered students datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Student/Mentor")]
        public async Task<IActionResult> GetStudentsForMentor([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetEntitiesForMentorAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }

        /// <summary>
        /// Gets the all filtered students datas for admin.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("Student/Admin")]
        public async Task<IActionResult> GetStudentsForAdmin([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetEntitiesForAdminAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }

        /// <summary>
        /// Gets the all filtered students datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student/Student")]
        public async Task<IActionResult> GetStudentsForStudent([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetEntitiesForStudentAsync(paginationParams.PageIndex,
                                                                                 paginationParams.RequestedItemCount,
                                                                                 paginationParams.OrderByProperty,
                                                                                 paginationParams.OrderByAscending,
                                                                                 paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }

        /// <summary>
        /// Gets the all filtered student datas for mentor.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Mentor")]
        [HttpPatch("Student/Mentor/{id}")]
        public async Task<IActionResult> GetStudentForMentorbyId([FromBody] Guid id)
        {
            var student = await _studentService.GetEntityForMentorAsync(id).ConfigureAwait(false);

            return Ok(student);
        }

        /// <summary>
        /// Gets the filtered student datas for admin.
        /// </summary>
        /// <returns></returns>
        ///[Authorize(Roles = "Admin")]
        [HttpPatch("Student/Admin/{id}")]
        public async Task<IActionResult> GetStudentForAdminbyId([FromBody] Guid id)
        {
            var student = await _studentService.GetEntityForAdminAsync(id).ConfigureAwait(false);

            return Ok(student);
        }

        /// <summary>
        /// Gets the filtered student datas for student.
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Student")]
        [HttpPatch("Student/Student/{id}")]
        public async Task<IActionResult> GetStudentForStudentbyId([FromBody] Guid id)
        {
            var student = await _studentService.GetEntityForStudentAsync(id).ConfigureAwait(false);

            return Ok(student);
        }

        /// <summary>
        /// Add <b><paramref name="addStudent"/></b> data to database.
        /// </summary>
        /// <param name="addStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPost("AddStudent")]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentDTO addStudent)
        {
            await _studentService.AddEntityAsync(addStudent).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Update <paramref name="updateStudent"/> data.
        /// </summary>
        /// <param name="updateStudent"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentDTO updateStudent)
        {
            await _studentService.UpdateEntityAsync(updateStudent).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete student data by <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Student/Delete/{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            await _studentService.DeleteEntityAsync(id).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Delete professions data by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Admin")]
        [HttpDelete("Student/Deletes/{ids}")]
        public async Task<IActionResult> DeleteStudents(List<Guid> ids)
        {
            await _studentService.DeleteEntitiesAsync(ids).ConfigureAwait(false);
            return Ok();
        }

    }
}
