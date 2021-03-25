using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Controllers
{
    public class UsefulLinkController : Controller
    {

        private readonly IUsefulLinkService _userfulLinkService;

        public UsefulLinkController(IUsefulLinkService usefulLinkService)
        {
            _userfulLinkService = usefulLinkService;
        }
        [HttpGet("GetLinks")]
        public async Task<IActionResult> GetStudentForAdmin([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var links = await _userfulLinkService.GetEntitiesForStudent(paginationParams.Spec).ConfigureAwait(false);
            return Ok(links);
        }
    }
}
