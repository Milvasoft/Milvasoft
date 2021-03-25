using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    public class MentorController : Controller
    {
        private readonly IMentorService _mentorService;

        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        [HttpGet("Mentor")]
        public async Task<IActionResult> GetStudentForAdmin([FromBody] PaginationParamsWithSpec<MentorSpec> paginationParams)
        {
            var students = await _mentorService.GetEntitiesForAdmin(paginationParams.Spec).ConfigureAwait(false);
            return Ok(students);
        }
    }
}
