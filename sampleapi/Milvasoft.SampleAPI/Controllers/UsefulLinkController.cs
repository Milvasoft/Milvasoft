using Microsoft.AspNetCore.Mvc;
using Milvasoft.API.DTOs;
using Milvasoft.SampleAPI.DTOs.UsefulLinkDTOs;
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
        [HttpPatch("GetLinks")]
        public async Task<IActionResult> GetUsefulLinks([FromBody] PaginationParamsWithSpec<UsefulLinkSpec> paginationParams)
        {
            var links = await _userfulLinkService.GetEntitiesForStudentAsync(paginationParams.PageIndex, paginationParams.RequestedItemCount, paginationParams.OrderByProperty, paginationParams.OrderByAscending, paginationParams.Spec).ConfigureAwait(false);
            return Ok(links);
        }
        [HttpPost("Add")]
        public async Task<IActionResult> AddUsefulLink(AddUsefulLinkDTO addUsefulLinkDTO)
        {
            await _userfulLinkService.AddEntityAsync(addUsefulLinkDTO).ConfigureAwait(false);
            return Ok();
        }
    }
}
