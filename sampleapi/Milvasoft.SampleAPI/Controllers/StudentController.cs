using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        /// <summary>
        /// Student controller constructor method.
        /// </summary>
        /// <param name="studentService"></param>
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all student for admin.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Students")]
        public async Task <IActionResult> GetStudentForAdmin([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetEntitiesForStudent(paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }

        /// <summary>
        /// Add student to database.
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddStudent(StudentDTO student)
        {
            await _studentService.AddEntityAsync(student).ConfigureAwait(false);
            return Ok();
        }

    }
}
