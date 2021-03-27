using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.Utils.Attributes.ActionFilters;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    /// <summary>
    /// Provided Mentor operations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MentorController : Controller
    {
        private readonly IMentorService _mentorService;

        /// <summary>
        /// Mentor constructor method for injection.
        /// </summary>
        /// <param name="mentorService"></param>
        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        /// <summary>
        /// The student list returns according to the <paramref name="paginationParams"/> sent.
        /// </summary>
        /// <param name="paginationParams"></param>
        /// <returns></returns>
        [HttpGet("Mentor")]
        public async Task<IActionResult> GetStudentForAdmin([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var students = await _mentorService.GetEntitiesForAdminAsync(paginationParams.Spec).ConfigureAwait(false);

            return Ok(students);
        }
        /// <summary>
        /// Add student to database .
        /// </summary>
        /// <param name="mentorDTO"></param>
        /// <returns></returns>
        [HttpPost("Mentor")]
        [OValidationFilter]
        public async Task<IActionResult> AddStudent([FromBody] AddMentorDTO mentorDTO)
        {
            await _mentorService.AddEntityAsync(mentorDTO).ConfigureAwait(false);
            return Ok();
        }
    }
}
