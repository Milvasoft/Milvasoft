using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Student operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
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
        public async Task<IActionResult> GetStudentForAdmin([FromBody] PaginationParamsWithSpec<StudentSpec> paginationParams)
        {
            var students = await _studentService.GetEntitiesForStudentAsync(paginationParams.PageIndex, paginationParams.RequestedItemCount, paginationParams.OrderByProperty, paginationParams.OrderByAscending, paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }

        /// <summary>
        /// Add student to database.
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        public async Task<IActionResult> AddStudent([FromBody] AddStudentDTO student)
        {
            await _studentService.AddEntityAsync(student).ConfigureAwait(false);
            return Ok();
        }

    }
}
